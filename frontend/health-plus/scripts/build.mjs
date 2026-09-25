// Chạy ng build và in nhịp thời gian + mã thoát — chẩn đoán build trên máy chủ deploy
// dừng giữa chừng mà không có thông báo lỗi.
import os from 'node:os';
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const ngCli = fileURLToPath(new URL('../node_modules/@angular/cli/bin/ng.js', import.meta.url));
const mb = (n) => `${Math.round(n / 1024 / 1024)} MB`;
const started = Date.now();
const elapsed = () => `${Math.round((Date.now() - started) / 1000)}s`;

const child = spawn(process.execPath, [ngCli, 'build', ...process.argv.slice(2)], {
  stdio: ['ignore', 'inherit', 'inherit'],
  env: { ...process.env, NG_BUILD_DEBUG_PERF: '1' },
});

const heartbeat = setInterval(() => {
  console.log(`[build] đang chạy ${elapsed()}, RAM trống ${mb(os.freemem())}`);
}, 5000);

child.on('error', (err) => {
  clearInterval(heartbeat);
  console.log(`[build] không chạy được ng build: ${err.message}`);
  process.exit(1);
});

child.on('exit', (code, signal) => {
  clearInterval(heartbeat);
  console.log(`[build] kết thúc sau ${elapsed()} — exit code ${code}, signal ${signal ?? 'không'}`);
  process.exit(code ?? 1);
});

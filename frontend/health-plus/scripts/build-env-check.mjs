// In thông tin môi trường build (Node, RAM, module native) trước khi chạy ng build —
// giúp chẩn đoán khi build trên máy chủ deploy thất bại mà không có thông báo lỗi.
import os from 'node:os';
import { createRequire } from 'node:module';
import { execFileSync } from 'node:child_process';

const require = createRequire(import.meta.url);
const mb = (n) => `${Math.round(n / 1024 / 1024)} MB`;

console.log(`[env] node ${process.version} ${process.platform}-${process.arch}`);
console.log(`[env] RAM tổng ${mb(os.totalmem())}, trống ${mb(os.freemem())}, CPU ${os.cpus().length}`);

function check(name, fn) {
  const start = Date.now();
  try {
    const detail = fn();
    console.log(`[ok]   ${name}${detail ? ` — ${detail}` : ''} (${Date.now() - start} ms)`);
  } catch (err) {
    console.log(`[FAIL] ${name} — ${err && err.message ? err.message.split('\n')[0] : err}`);
  }
}

check('esbuild', () => execFileSync(process.execPath, [require.resolve('esbuild/bin/esbuild'), '--version']).toString().trim());
check('lightningcss', () => {
  const { transform } = require('lightningcss');
  transform({ filename: 'a.css', code: Buffer.from('.a{color:red}'), minify: true });
  return 'transform OK';
});
check('@tailwindcss/oxide', () => {
  const { Scanner } = require('@tailwindcss/oxide');
  new Scanner({ sources: [] });
  return 'Scanner OK';
});

console.log(`[env] RAM process sau kiểm tra: ${mb(process.memoryUsage().rss)}`);

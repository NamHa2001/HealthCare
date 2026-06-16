using HealthCare.Domain.Entities.Medications;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Seeds;

public static class DrugCatalogSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.DrugCatalog.AnyAsync()) return;

        var drugs = new List<DrugCatalog>
        {
            // Kháng sinh
            DrugCatalog.Create("Amoxicillin", "Amoxil", "Kháng sinh Penicillin", "[\"250mg\",\"500mg\"]"),
            DrugCatalog.Create("Azithromycin", "Zithromax", "Kháng sinh Macrolide", "[\"250mg\",\"500mg\"]"),
            DrugCatalog.Create("Ciprofloxacin", "Cipro", "Kháng sinh Quinolone", "[\"250mg\",\"500mg\",\"750mg\"]"),
            DrugCatalog.Create("Doxycycline", null, "Kháng sinh Tetracycline", "[\"100mg\"]"),
            DrugCatalog.Create("Metronidazole", "Flagyl", "Kháng sinh/Kháng ký sinh trùng", "[\"250mg\",\"500mg\"]"),
            DrugCatalog.Create("Cefuroxime", "Zinnat", "Kháng sinh Cephalosporin", "[\"125mg\",\"250mg\",\"500mg\"]"),
            DrugCatalog.Create("Clarithromycin", "Klacid", "Kháng sinh Macrolide", "[\"250mg\",\"500mg\"]"),
            DrugCatalog.Create("Trimethoprim/Sulfamethoxazole", "Bactrim", "Kháng sinh Sulfonamide", "[\"480mg\",\"960mg\"]"),

            // Hạ sốt / Giảm đau
            DrugCatalog.Create("Paracetamol", "Panadol", "Hạ sốt/Giảm đau", "[\"325mg\",\"500mg\",\"650mg\"]"),
            DrugCatalog.Create("Ibuprofen", "Advil", "NSAID Giảm đau/Kháng viêm", "[\"200mg\",\"400mg\",\"600mg\"]"),
            DrugCatalog.Create("Aspirin", null, "NSAID/Chống kết tập tiểu cầu", "[\"81mg\",\"325mg\"]"),
            DrugCatalog.Create("Diclofenac", "Voltaren", "NSAID Kháng viêm", "[\"25mg\",\"50mg\",\"75mg\"]"),
            DrugCatalog.Create("Meloxicam", "Mobic", "NSAID Kháng viêm", "[\"7.5mg\",\"15mg\"]"),
            DrugCatalog.Create("Naproxen", "Aleve", "NSAID Kháng viêm", "[\"250mg\",\"500mg\"]"),

            // Tiêu hóa
            DrugCatalog.Create("Omeprazole", "Losec", "Ức chế bơm proton", "[\"10mg\",\"20mg\",\"40mg\"]"),
            DrugCatalog.Create("Pantoprazole", "Protonix", "Ức chế bơm proton", "[\"20mg\",\"40mg\"]"),
            DrugCatalog.Create("Esomeprazole", "Nexium", "Ức chế bơm proton", "[\"20mg\",\"40mg\"]"),
            DrugCatalog.Create("Ranitidine", "Zantac", "Kháng H2", "[\"150mg\",\"300mg\"]"),
            DrugCatalog.Create("Metoclopramide", "Primperan", "Chống nôn/Kích thích tiêu hóa", "[\"10mg\"]"),
            DrugCatalog.Create("Domperidone", "Motilium", "Chống nôn", "[\"10mg\"]"),
            DrugCatalog.Create("Loperamide", "Imodium", "Chống tiêu chảy", "[\"2mg\"]"),
            DrugCatalog.Create("Lactulose", null, "Trị táo bón", "[\"10g/15ml\"]"),
            DrugCatalog.Create("Smecta", null, "Bảo vệ niêm mạc ruột", "[\"3g/gói\"]"),

            // Tim mạch / Huyết áp
            DrugCatalog.Create("Amlodipine", "Norvasc", "Chẹn kênh canxi", "[\"5mg\",\"10mg\"]"),
            DrugCatalog.Create("Enalapril", "Vasotec", "Ức chế ACE", "[\"5mg\",\"10mg\",\"20mg\"]"),
            DrugCatalog.Create("Losartan", "Cozaar", "Chẹn thụ thể AT1", "[\"25mg\",\"50mg\",\"100mg\"]"),
            DrugCatalog.Create("Valsartan", "Diovan", "Chẹn thụ thể AT1", "[\"80mg\",\"160mg\",\"320mg\"]"),
            DrugCatalog.Create("Bisoprolol", "Concor", "Chẹn beta", "[\"2.5mg\",\"5mg\",\"10mg\"]"),
            DrugCatalog.Create("Metoprolol", "Lopressor", "Chẹn beta", "[\"25mg\",\"50mg\",\"100mg\"]"),
            DrugCatalog.Create("Atenolol", "Tenormin", "Chẹn beta", "[\"25mg\",\"50mg\",\"100mg\"]"),
            DrugCatalog.Create("Hydrochlorothiazide", null, "Lợi tiểu Thiazide", "[\"12.5mg\",\"25mg\"]"),
            DrugCatalog.Create("Furosemide", "Lasix", "Lợi tiểu quai", "[\"20mg\",\"40mg\",\"80mg\"]"),
            DrugCatalog.Create("Spironolactone", "Aldactone", "Lợi tiểu tiết kiệm kali", "[\"25mg\",\"50mg\",\"100mg\"]"),

            // Đái tháo đường
            DrugCatalog.Create("Metformin", "Glucophage", "Biguanide hạ đường huyết", "[\"500mg\",\"850mg\",\"1000mg\"]"),
            DrugCatalog.Create("Glibenclamide", "Daonil", "Sulfonylurea hạ đường huyết", "[\"2.5mg\",\"5mg\"]"),
            DrugCatalog.Create("Glimepiride", "Amaryl", "Sulfonylurea hạ đường huyết", "[\"1mg\",\"2mg\",\"4mg\"]"),
            DrugCatalog.Create("Sitagliptin", "Januvia", "Ức chế DPP-4", "[\"25mg\",\"50mg\",\"100mg\"]"),

            // Mỡ máu
            DrugCatalog.Create("Atorvastatin", "Lipitor", "Statin hạ mỡ máu", "[\"10mg\",\"20mg\",\"40mg\",\"80mg\"]"),
            DrugCatalog.Create("Rosuvastatin", "Crestor", "Statin hạ mỡ máu", "[\"5mg\",\"10mg\",\"20mg\",\"40mg\"]"),
            DrugCatalog.Create("Simvastatin", "Zocor", "Statin hạ mỡ máu", "[\"10mg\",\"20mg\",\"40mg\"]"),

            // Hô hấp
            DrugCatalog.Create("Salbutamol", "Ventolin", "Giãn phế quản Beta2", "[\"2mg\",\"4mg\",\"100mcg/puff\"]"),
            DrugCatalog.Create("Montelukast", "Singulair", "Kháng leukotriene", "[\"4mg\",\"5mg\",\"10mg\"]"),
            DrugCatalog.Create("Budesonide", "Pulmicort", "Corticosteroid hô hấp", "[\"200mcg/puff\",\"400mcg/puff\"]"),
            DrugCatalog.Create("Loratadine", "Claritin", "Kháng histamine H1", "[\"10mg\"]"),
            DrugCatalog.Create("Cetirizine", "Zyrtec", "Kháng histamine H1", "[\"5mg\",\"10mg\"]"),
            DrugCatalog.Create("Fexofenadine", "Allegra", "Kháng histamine H1", "[\"60mg\",\"120mg\",\"180mg\"]"),

            // Thần kinh / Tâm thần
            DrugCatalog.Create("Amitriptyline", "Elavil", "Chống trầm cảm TCA", "[\"10mg\",\"25mg\",\"50mg\"]"),
            DrugCatalog.Create("Sertraline", "Zoloft", "Chống trầm cảm SSRI", "[\"25mg\",\"50mg\",\"100mg\"]"),
            DrugCatalog.Create("Diazepam", "Valium", "Benzodiazepin", "[\"2mg\",\"5mg\",\"10mg\"]"),
            DrugCatalog.Create("Alprazolam", "Xanax", "Benzodiazepin", "[\"0.25mg\",\"0.5mg\",\"1mg\"]"),
            DrugCatalog.Create("Gabapentin", "Neurontin", "Chống động kinh/Thần kinh", "[\"100mg\",\"300mg\",\"400mg\"]"),
            DrugCatalog.Create("Pregabalin", "Lyrica", "Chống động kinh/Thần kinh", "[\"25mg\",\"50mg\",\"75mg\",\"150mg\",\"300mg\"]"),

            // Vitamin / Khoáng chất
            DrugCatalog.Create("Vitamin C", null, "Vitamin", "[\"100mg\",\"250mg\",\"500mg\",\"1000mg\"]"),
            DrugCatalog.Create("Vitamin D3", null, "Vitamin", "[\"400IU\",\"1000IU\",\"2000IU\",\"4000IU\"]"),
            DrugCatalog.Create("Vitamin B Complex", null, "Vitamin nhóm B", null),
            DrugCatalog.Create("Sắt (Ferrous Sulfate)", null, "Bổ sung sắt", "[\"60mg\",\"325mg\"]"),
            DrugCatalog.Create("Canxi + Vitamin D", null, "Bổ sung canxi", "[\"500mg+200IU\",\"600mg+400IU\"]"),
            DrugCatalog.Create("Kẽm", null, "Vi khoáng", "[\"10mg\",\"15mg\",\"30mg\"]"),
            DrugCatalog.Create("Acid Folic", null, "Vitamin B9", "[\"0.4mg\",\"1mg\",\"5mg\"]"),

            // Nội tiết / Hormone
            DrugCatalog.Create("Levothyroxine", "Synthroid", "Hormone tuyến giáp", "[\"25mcg\",\"50mcg\",\"75mcg\",\"100mcg\"]"),
            DrugCatalog.Create("Prednisolone", "Predate", "Corticosteroid", "[\"5mg\",\"10mg\",\"20mg\",\"30mg\"]"),
            DrugCatalog.Create("Dexamethasone", null, "Corticosteroid", "[\"0.5mg\",\"1mg\",\"4mg\"]"),
            DrugCatalog.Create("Methylprednisolone", "Medrol", "Corticosteroid", "[\"4mg\",\"8mg\",\"16mg\"]"),

            // Khác
            DrugCatalog.Create("Allopurinol", "Zyloprim", "Hạ acid uric (Gout)", "[\"100mg\",\"300mg\"]"),
            DrugCatalog.Create("Colchicine", null, "Trị Gout cấp", "[\"0.5mg\",\"1mg\"]"),
            DrugCatalog.Create("Warfarin", "Coumadin", "Chống đông máu", "[\"1mg\",\"2mg\",\"5mg\",\"10mg\"]"),
            DrugCatalog.Create("Clopidogrel", "Plavix", "Chống kết tập tiểu cầu", "[\"75mg\"]"),
            DrugCatalog.Create("Omeprazole", "Prilosec", "Ức chế bơm proton", "[\"20mg\",\"40mg\"]"),
        };

        db.DrugCatalog.AddRange(drugs);
        await db.SaveChangesAsync();
    }
}

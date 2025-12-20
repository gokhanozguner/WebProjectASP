using GokhanOzgunerWEB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using System.Text.RegularExpressions; // Regex kullanımı için

namespace GokhanOzgunerWEB.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly IConfiguration _configuration;

        public AIController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new AIRecommendationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AIRecommendationViewModel model)
        {
            // 1. Validasyon Kontrolü: Ne fotoğraf var ne de veri girilmişse hata ver
            bool bodyDataExists = model.Boy.HasValue && model.Kilo.HasValue;
            bool photoExists = model.Foto != null && model.Foto.Length > 0;

            if (!ModelState.IsValid && !photoExists && !bodyDataExists)
            {
                ModelState.AddModelError("", "Lütfen analiz için ya vücut bilgilerinizi girin ya da bir fotoğraf yükleyin.");
                return View(model);
            }

            try
            {
                // --- BÖLÜM 1: GEMINI İLE METİN ANALİZİ VE RESİM TARİFİ HAZIRLAMA ---

                var apiKey = _configuration["Gemini:ApiKey"];
                var googleAi = new GoogleAI(apiKey: apiKey);

                // Flash modelini seçiyorum
                var generativeModel = googleAi.GenerativeModel(model: "gemini-2.5-flash");

                // Prompt hazırlığı
                string userStats = "";
                if (bodyDataExists)
                {
                    double boyMetre = model.Boy.Value / 100.0;
                    double vki = model.Kilo.Value / (boyMetre * boyMetre);
                    userStats = $"\nKullanıcı Verileri:\n- Boy: {model.Boy} cm\n- Kilo: {model.Kilo} kg\n- VKİ: {vki:F2}\n- Yaş: {model.Yas}\n- Cinsiyet: {model.Cinsiyet}\n- Hedef: {model.Hedef}";
                }

                // AI'a gönderilecek ana komut
                string systemPrompt = $@"Sen profesyonel bir fitness antrenörü ve diyetisyensin.
                                        {userStats}
                                        
                                        GÖREVİN:
                                        1. Türkçe olarak kişiye özel, detaylı bir fitness ve beslenme programı hazırla.
                                        2. Programın sonunda, bu kişinin hedefine ulaştığında (örneğin 6 ay sonra) nasıl görüneceğini hayal et.
                                        3. Bu hayali görüntüyü çizecek bir yapay zeka (Image Generator) için İNGİLİZCE, çok detaylı bir 'Prompt' (resim tarifi) yaz.
                                        4. Yazdığın İngilizce prompt'u tam olarak [IMAGE_PROMPT_START] ve [IMAGE_PROMPT_END] etiketleri arasına koy.
                                        
                                        Örnek Resim Tarifi Formatı:
                                        [IMAGE_PROMPT_START] a realistic photo of a fit man working out in a gym, muscular arms, sweat, cinematic lighting, 8k resolution [IMAGE_PROMPT_END]
                                        
                                        Lütfen önce programı yaz, en sona resim tarifini ekle.";

                var parts = new List<IPart>();
                parts.Add(new TextData { Text = systemPrompt });

                // Eğer fotoğraf yüklendiyse onu da analize ekle
                if (photoExists)
                {
                    using var memoryStream = new MemoryStream();
                    await model.Foto.CopyToAsync(memoryStream);
                    var base64ImageInput = Convert.ToBase64String(memoryStream.ToArray());
                    parts.Add(new InlineData { MimeType = model.Foto.ContentType, Data = base64ImageInput });
                }

                // Gemini'ye isteği gönder
                var request = new GenerateContentRequest { Contents = new List<Content> { new Content { Role = "user", Parts = parts } } };
                var response = await generativeModel.GenerateContent(request);
                string fullResponseText = response.Text;

                // --- BÖLÜM 2: YANITI AYIKLAMA (METİN vs RESİM TARİFİ) ---

                string imagePrompt = "";

                // Etiketler arasındaki İngilizce tarifi bul
                var match = Regex.Match(fullResponseText, @"\[IMAGE_PROMPT_START\](.*?)\[IMAGE_PROMPT_END\]", RegexOptions.Singleline);

                if (match.Success)
                {
                    imagePrompt = match.Groups[1].Value.Trim();
                    // Kullanıcıya sadece Türkçe metni göstermek için etiketli kısmı sil
                    model.Oneri = fullResponseText.Replace(match.Value, "").Trim();
                }
                else
                {
                    // AI etiket koymayı unuttuysa varsayılan bir tarif oluştur
                    model.Oneri = fullResponseText;
                    string cinsiyet = model.Cinsiyet == "Kadın" ? "woman" : "man";
                    imagePrompt = $"fitness transformation, fit {cinsiyet} bodybuilder, gym setting, realistic photography, 8k, highly detailed, cinematic lighting";
                }

                // --- AI İLE GÖRSEL OLUŞTURMA ---

                if (photoExists && !string.IsNullOrEmpty(imagePrompt))
                {
                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            // Prompt'u URL için güvenli hale getir
                            var encodedPrompt = Uri.EscapeDataString(imagePrompt);

                            // Pollinations API URL'si

                            string imageUrl = $"https://image.pollinations.ai/prompt/{encodedPrompt}?width=512&height=512&model=flux&nologo=true&seed={new Random().Next(1, 99999)}";

                            // Resmi indir
                            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                            // Base64'e çevirip ViewModel'e at
                            model.GeneratedImageBase64 = Convert.ToBase64String(imageBytes);
                        }
                    }
                    catch (Exception imgEx)
                    {
                        // Resim servisinde hata olursa kullanıcıya hissettirme, sadece metni göster

                        model.Oneri += "\n\n*(Not: Görsel sunucusu şu an yoğun olduğu için temsili resim oluşturulamadı, ancak programınız yukarıdadır.)*";
                    }
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "AI servisine bağlanırken bir hata oluştu: " + ex.Message);
                return View(model);
            }

            return View(model);
        }
    }
}
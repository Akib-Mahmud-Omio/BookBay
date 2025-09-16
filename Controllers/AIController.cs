using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using DocumentFormat.OpenXml.Packaging;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Newtonsoft.Json;

namespace E_Book_Store_1.Controllers
{
    public class AIController : Controller
    {
        // GET: AI
        public ActionResult Index()
        {
            return View();
        }

        // POST: AI/Process
        // Keep ValidateAntiForgeryToken for security. JS will send token in header.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Process()
        {
            try
            {
                // Read prompt from Request.Form - this is more reliable with multipart/form-data
                var prompt = Request.Form["prompt"] ?? string.Empty;

                var uploadedFiles = new List<HttpPostedFileBase>();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var f = Request.Files[i];
                    if (f != null && f.ContentLength > 0)
                        uploadedFiles.Add(f);
                }

                var combinedFileText = new StringBuilder();
                foreach (var file in uploadedFiles)
                {
                    var extracted = await ExtractTextFromFileAsync(file);
                    combinedFileText.AppendLine($"File: {file.FileName}\n{extracted}\n---\n");
                }

                var finalPrompt = new StringBuilder();
                finalPrompt.AppendLine("User prompt:");
                finalPrompt.AppendLine(prompt);
                if (combinedFileText.Length > 0)
                {
                    finalPrompt.AppendLine("\nAttached document text:");
                    finalPrompt.AppendLine(combinedFileText.ToString());
                }

                var aiReply = await QueryOpenRouterChatAsync(finalPrompt.ToString());

                return Json(new { success = true, reply = aiReply });
            }
            catch (Exception ex)
            {
                // Return a JSON error (don’t leak sensitive stack trace in production)
                return Json(new { success = false, error = ex.Message });
            }
        }

        private async Task<string> ExtractTextFromFileAsync(HttpPostedFileBase postedFile)
        {
            if (postedFile == null) return "";

            var fileName = postedFile.FileName ?? "";
            var ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();

            var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ext);
            postedFile.SaveAs(tempPath);

            try
            {
                if (ext == ".txt")
                {
                    return await Task.Run(() => System.IO.File.ReadAllText(tempPath, Encoding.UTF8));
                }
                else if (ext == ".docx")
                {
                    return ReadDocxText(tempPath);
                }
                else if (ext == ".pdf")
                {
                    return ReadPdfText(tempPath);
                }
                else
                {
                    return $"[Unsupported file type {ext}]";
                }
            }
            finally
            {
                try { System.IO.File.Delete(tempPath); } catch { /* ignore */ }
            }
        }

        private string ReadDocxText(string path)
        {
            try
            {
                using (var wordDoc = WordprocessingDocument.Open(path, false))
                {
                    var body = wordDoc.MainDocumentPart?.Document?.Body;
                    return body?.InnerText ?? "";
                }
            }
            catch (Exception ex)
            {
                return "[Error reading docx: " + ex.Message + "]";
            }
        }

        private string ReadPdfText(string path)
        {
            try
            {
                StringBuilder text = new StringBuilder();
                using (var reader = new iTextSharp.text.pdf.PdfReader(path))
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        var page = PdfTextExtractor.GetTextFromPage(reader, i);
                        text.AppendLine(page);
                    }
                }
                return text.ToString();
            }
            catch (Exception ex)
            {
                return "[Error reading pdf: " + ex.Message + "]";
            }
        }

        /// <summary>
        /// Calls OpenRouter chat completions endpoint to query the specified model.
        /// Endpoint: https://openrouter.ai/api/v1/chat/completions
        /// Model used (as requested): deepseek/deepseek-chat-v3.1:free
        /// See OpenRouter API docs for details. :contentReference[oaicite:1]{index=1}
        /// </summary>
        private async Task<string> QueryOpenRouterChatAsync(string prompt)
        {
            var apiKey = ConfigurationManager.AppSettings["OpenRouterApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("OpenRouter API key is not configured. Add OpenRouterApiKey in Web.config appSettings.");

            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var requestBody = new
                {
                    model = "deepseek/deepseek-chat-v3.1:free",
                    messages = new[] {
                        new { role = "system", content = "You are an assistant that summarizes and answers queries based on given text." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.2,
                    max_tokens = 2000
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var resp = await http.PostAsync("https://openrouter.ai/api/v1/chat/completions",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                var respStr = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                {
                    // include response content to aid debugging (but be careful in production not to leak secrets)
                    throw new Exception($"AI service error: {resp.StatusCode} - {respStr}");
                }

                dynamic parsed = JsonConvert.DeserializeObject(respStr);

                // OpenRouter returns choices[].message.content similar to OpenAI's chat response.
                // Defensive access in case the JSON shape is slightly different.
                try
                {
                    string assistant = parsed.choices[0].message.content;
                    if (assistant == null)
                    {
                        // fallback to older schema or provider-specific shape
                        assistant = parsed.choices[0].message?.content ?? parsed.choices[0]?.text ?? respStr;
                    }
                    return assistant;
                }
                catch
                {
                    // If parsing fails, return the raw response for debugging
                    return respStr;
                }
            }
        }
    }
}

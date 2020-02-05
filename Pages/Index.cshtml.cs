using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace IMDBReview.Pages
{
    [BindProperties(SupportsGet = true)]
    public class IndexModel : PageModel
    {
        public string Sentiment { get; set; }
        public string Review { get; set; }
        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPost()
        {
            string s = Request.Form["review"];
            string result = await InvokeRequestResponseService(s).ConfigureAwait(false);
            JObject jsonRes = JObject.Parse(result);
            string label = (string)jsonRes["Results"]["Sentiment Output"][0]["Scored Labels"];
            string output;
            if (label == "1") 
            {
                output = "Positive";
            }
            else
            {
                output = "Negative";
            }
            Sentiment = output;
            Review = s;
            return Page();
        }
        static async Task<string> InvokeRequestResponseService(string s)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "Input",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "review", s
                                            },
                                }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };

                const string apiKey = "25cY+7WhNzcGZsW5P86Hdmu0clHK1AIIkKeydPGIuYYgDwS2lVnqRU35NtGDVj51VSFrFZrrUCrQwaktMKTfwg=="; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri("https://asiasoutheast.services.azureml.net/subscriptions/c7ee295b73a94f7aa7155193d3f1b2c8/services/398e2ce8fa2c4be498bbe2fd049e4a21/execute?api-version=2.0&format=swagger");

                // WARNING: The 'await' statement below can result in a deadlock
                // if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false)
                // so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)

                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Result: {0}", result);
                    return result;
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp,
                    // which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
                }
            }
        }
    }
}

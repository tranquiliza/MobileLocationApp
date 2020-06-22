using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace POC.MobileLocation.Services
{
    public class StreamLabelsApiService : IDisposable
    {
        private readonly HttpClient client;
        private readonly string apiAddress;

        public StreamLabelsApiService(string apiAddress)
        {
            client = new HttpClient();
            this.apiAddress = apiAddress;
        }

        public async Task PostUpdateToApi(PositionModel positionModel)
        {
            var url = apiAddress + "/location/tranquiliza";
            var json = JsonConvert.SerializeObject(positionModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                await client.PostAsync(url, content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
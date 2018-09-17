using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NBitcoin.SolarCoin.Tests
{
    public class BaseTest : IDisposable
    {
        public BaseTest()
        {
            using (var file = File.OpenText("Properties\\launchSettings.json"))
            {
                var reader = new JsonTextReader(file);
                var jObject = JObject.Load(reader);

                var variables = jObject
                    .GetValue("profiles")
                    //select a proper profile here
                    .SelectMany(profiles => profiles.Children())
                    .SelectMany(profile => profile.Children<JProperty>())
                    .Where(prop => prop.Name == "environmentVariables")
                    .SelectMany(prop => prop.Value.Children<JProperty>())
                    .ToList();

                foreach (var variable in variables)
                {
                    Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                }
            }
        }

        public void Dispose()
        {
            // ... clean up
        }
    }

    public static class EnvInitializer
    {
        private static object _lock = new object();
        private static BaseTest _baseTest = null;

        public static void Init()
        {
            lock (_lock)
            {
                if (_baseTest == null)
                {
                    _baseTest = new BaseTest();
                }
            }
        }
    }
}

using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HienNguyen_FPT_TestProject
{
    public enum WeatherAnswer
    {
        No,
        Yes
    }

    public partial class WeatherPrompt : Form
    {
        const string API_ENDPOINT = "http://api.weatherstack.com/current";

        const string CAPTION_DIALOG_RESULT = "Weather Result";
        const string CAPTION_DIALOG_ERROR = "Error";

        const string MESSAGE_INVALID_INPUT = "Zip Code must be filled to get weather result! Please input it correctly!";

        const string ACCESS_KEY = "610acf4c1d203448cd6f671955c5e8aa";

        const int DEFAULT_REQUEST_TIMEOUT = 5000;

        readonly Dictionary<int, string> RAIN_DICTIONARY = new Dictionary<int, string>()
        {
            { 293, "Patchy light rain" },
            { 296, "Light rain" },
            { 299, "Moderate rain at times" },
            { 302, "Moderate rain" },
            { 305, "Heavy rain at times" },
            { 308, "Heavy rain" },
            { 311, "Light freezing rain" }
        };

        public WeatherPrompt()
        {
            InitializeComponent();
        }

        private void WeatherPrompt_Load(object sender, EventArgs e)
        {

        }

        private void ShowResultDialog(string info)
        {
            Form dialog = new Form()
            {
                Width = 500,
                Height = 300,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = CAPTION_DIALOG_RESULT,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label messageLbl = new Label() { Left = 10, Top = 20, Width = 400, Height = 200, Text = info };
            Button confirmationBtn = new Button() { Text = "OK", Left = 225, Width = 50, Top = 220, DialogResult = DialogResult.OK };
            confirmationBtn.Click += (sender, e) => { dialog.Close(); };
            dialog.Controls.Add(confirmationBtn);
            dialog.Controls.Add(messageLbl);
            dialog.AcceptButton = confirmationBtn;

            dialog.ShowDialog();
        }

        private void ShowErrorDialog(string caption, string info)
        {
            Form dialog = new Form()
            {
                Width = 250,
                Height = 250,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label messageLbl = new Label() { Left = 10, Top = 20, Width = 180, Height = 100, Text = info };
            Button confirmationBtn = new Button() { Text = "OK", Left = 75, Width = 50, Top = 150, DialogResult = DialogResult.OK };
            confirmationBtn.Click += (sender, e) => { dialog.Close(); };
            dialog.Controls.Add(confirmationBtn);
            dialog.Controls.Add(messageLbl);
            dialog.AcceptButton = confirmationBtn;

            dialog.ShowDialog();
        }

        private string GetWeatherAnswerMessage(string question, bool answer, string additionalText = null)
        {
            var result = $"{question}\n" +
                $"Answer: {(WeatherAnswer)Convert.ToInt32(answer)}";
            if (!string.IsNullOrEmpty(additionalText)){
                result += ". " + additionalText;
            }
            result += "\n\n\n";
            return result;
        }

        private async void ExecuteBtn_Click(object sender, EventArgs e)
        {
            var zipCode = ZipCodeTxt.Text;

            // Validate input
            if (string.IsNullOrWhiteSpace(zipCode))
            {
                ShowErrorDialog(CAPTION_DIALOG_ERROR, MESSAGE_INVALID_INPUT);
                return;
            }

            var client = new RestClient(API_ENDPOINT);
            var request = new RestRequest()
                .AddQueryParameter("access_key", ACCESS_KEY)
                .AddQueryParameter("query", zipCode);
            request.Timeout = DEFAULT_REQUEST_TIMEOUT;
            var response = await client.ExecuteAsync(request);

            // Unsuccessful request
            if (!response.IsSuccessful)
            {
                ShowErrorDialog(CAPTION_DIALOG_ERROR, "No");
                return;
            }

            // Successful request, extract the content
            // See more: https://weatherstack.com/documentation
            var responseContent = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
            string infoMsg = string.Empty;
            if (responseContent.ContainsKey("success") && Boolean.TryParse(responseContent["success"].ToString(), out var success) && !success)
            {
                var errorContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent["error"].ToString());
                infoMsg = $"Error code: {errorContent["code"]}\n\n" +
                    $"Error type: {errorContent["type"]}\n\n" +
                    $"Info: {errorContent["info"]}";
                ShowErrorDialog(CAPTION_DIALOG_ERROR, infoMsg);
                return;
            }

            if (!responseContent.ContainsKey("current"))
            {
                return;
            }

            var currentWeatherInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent["current"].ToString());
            var weatherCode = int.Parse(currentWeatherInfo["weather_code"].ToString());
            var hasRain = RAIN_DICTIONARY.ContainsKey(weatherCode);
            var rainInfo = hasRain ? RAIN_DICTIONARY[weatherCode] : "No rain";
            var uvIndex = int.Parse(currentWeatherInfo["uv_index"].ToString());
            var windSpeed = int.Parse(currentWeatherInfo["wind_speed"].ToString());

            infoMsg += GetWeatherAnswerMessage($"1. Should I go outside?", !hasRain, $"Weather Code: {weatherCode} ({rainInfo})");
            
            infoMsg += GetWeatherAnswerMessage($"2. Should I wear sunscreen?", uvIndex > 3, $"UV Index: {uvIndex}");
            
            infoMsg += GetWeatherAnswerMessage($"3. Can I fly my kite?", !hasRain && windSpeed > 15, $"Weather Code: {weatherCode} ({rainInfo}), Wind Speed: {windSpeed}");

            ShowResultDialog(infoMsg);
        }
    }
}

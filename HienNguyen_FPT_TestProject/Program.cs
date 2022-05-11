using System.Windows.Forms;

namespace HienNguyen_FPT_TestProject
{
    /// <summary>
    /// Prompt user for zipcode
    /// Call this service to get the weather
    /// http://api.weatherstack.com/current?access_key=610acf4c1d203448cd6f671955c5e8aa&query=30076
    /// Answer for the user the following questions:
    /// - Should I go outside?
    /// + Yes if it’s not raining
    /// + No if it’s raining
    /// - Should I wear sunscreen?
    /// + If UV index above 3 then YES
    /// - Can I fly my kite?
    /// + Yes if not raining and wind speed over 15
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WeatherPrompt());
        }
    }
}

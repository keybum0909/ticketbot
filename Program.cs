using OpenQA.Selenium;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Chrome;
using Tesseract;

class Program
{
    static void Main(string[] args)
    {
        var options = new ChromeOptions();
        options.AddArgument("--ignore-certificate-errors");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        //options.AddArgument("--headless"); // 若需顯示 UI，移除此行
        var driver = new ChromeDriver(options);
        
        // 1. 開啟拓元網站登入頁
        driver.Navigate().GoToUrl("https://tixcraft.com/login");

        // 2. 點選 Facebook 登入（可能需判斷是否已有 cookie）
        var fbBtn = driver.FindElement(By.Id("facebook"));
        fbBtn.Click();

        // 3. Facebook 登入頁，填寫帳密
        driver.FindElement(By.Id("email")).SendKeys("wendy91520@yahoo.com.tw");
        driver.FindElement(By.Id("pass")).SendKeys("@wendy**91520@");
        driver.FindElement(By.Name("login")).Click();

        // 4. 驗證碼辨識
        // var captchaImg = driver.FindElement(By.CssSelector("img[src*='captcha']"));
        // var captchaUrl = captchaImg.GetAttribute("src");
        // if (!string.IsNullOrEmpty(captchaUrl))
        // {
        //     driver.Navigate().GoToUrl(captchaUrl);
        // }
        // else
        // {
        //     Console.WriteLine("URL 為 null 或空字串");
        // }
        // Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
        // screenshot.SaveAsFile("captcha.png");

        // var ocrEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        // using var img = Pix.LoadFromFile("captcha.png");
        // using var page = ocrEngine.Process(img);
        // var result = page.GetText();
        // Console.WriteLine($"辨識結果：{result}");

        // 5. 等待登入完成並回到拓元頁
        //System.Threading.Thread.Sleep(5000);

        // 6. 取得 Cookie（包含 SESSID）
        // 持續等待直到網址包含 "tixcraft.com"
        string currentUrl = "";
        while (true)
        {
            currentUrl = driver.Url;
            Console.WriteLine($"目前網址：{currentUrl}");

            if (currentUrl.Contains("https://tixcraft.com"))
            {
                driver.FindElement(By.Id("onetrust-accept-btn-handler")).Click();
                break; // 網址符合條件就跳出迴圈
            }

            Thread.Sleep(1000); // 每秒檢查一次
        }


        // 執行後續動作
        Console.WriteLine("登入成功，已回到拓元網站");

        var cookies = driver.Manage().Cookies.AllCookies;
        Console.WriteLine("目前取得 cookies:");
        foreach (var cookie in cookies)
        {
            Console.WriteLine($"{cookie.Name} = {cookie.Value}");
        }

        driver.Navigate().GoToUrl("https://tixcraft.com/activity/detail/25_valley");
        driver.FindElement(By.CssSelector("li.buy a")).Click();

        


        // 6. 把 SESSID 存起來用來打 API
        //driver.Quit();
    }
}

using OpenQA.Selenium;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Chrome;
using Tesseract;

class Program
{
    static void Main(string[] args)
    {
        // 購票網址
        string ticketUrl = "https://tixcraft.com/activity/detail/25_wubaikh";
        // 日期
        string date = "2025/11/22 (六) 19:30";
        //座位區
        string seat = "特2區4200";
        //張數
        int quantity = 4;

        //信用卡內容
        string cardNumber = "1234567891234567";
        string cardMonth = "3";
        string cardYear = "29";

        var options = new ChromeOptions();
        options.AddArgument("--ignore-certificate-errors");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        //options.AddArgument("--headless"); // 若需顯示 UI，移除此行
        var driver = new ChromeDriver(options);
        
        // 1. 開啟拓元網站登入頁
        driver.Navigate().GoToUrl("https://tixcraft.com/login");

        // 2. 點選 Facebook 登入（可能需判斷是否已有 cookie）
        driver.FindElement(By.Id("facebook")).Click();

        // 3. Facebook 登入頁，填寫帳密
        driver.FindElement(By.Id("email")).SendKeys("mail@yahoo.com.tw");
        driver.FindElement(By.Id("pass")).SendKeys("P@ssw0rd");
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
        while (true)
        {
            if (driver.Url == ticketUrl)
            {
                driver.FindElement(By.Id("onetrust-accept-btn-handler")).Click();
                WarStart(driver, date, seat, quantity);

                PayTicket(driver, cardNumber, cardMonth, cardYear);
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

        //driver.Quit();
    }

    public static void WarStart(IWebDriver driver, string date, string seat, int quantity){
        //7. 立即購票、點選場次
        var link = driver.FindElement(By.CssSelector("li.buy a"));
        string href = link.GetAttribute("href");
        driver.Navigate().GoToUrl(href);

        var rows = driver.FindElements(By.TagName("tr"));
        IWebElement targetRow = null;
        Console.WriteLine($"找到 {rows.Count} 行列");

        foreach (var row in rows)
        {
            if (row.Text.Contains(date))
            {
                targetRow = row;
                targetRow.FindElement(By.CssSelector("button[data-href]")).Click();
                break;
            }
        }

        //8. 點選座位 (電腦選位)
        var li = driver.FindElement(By.XPath($"//li[a[contains(., '{seat}')]]"));
        li.FindElement(By.TagName("a")).Click();

        //9. 點選張數與驗證碼
        //driver.FindElement(By.XPath($"//select[option[text()='{quantity}']]"));
        var select = driver.FindElement(By.TagName("select"));
        select.FindElement(By.CssSelector($"option[value='{quantity}']")).Click();
        var checkbox = driver.FindElement(By.Id("TicketForm_agree"));
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        js.ExecuteScript("arguments[0].click();", checkbox);

        // string code = GetCaptchaText(driver);
        // Console.WriteLine($"辨識出的驗證碼：{code}");

    }

    public static string GetCaptchaText(IWebDriver driver)
    {
        // 取得驗證碼圖片元素
        var captchaImage = driver.FindElement(By.Id("TicketForm_verifyCode-image"));
        var imageSrc = captchaImage.GetAttribute("src");

        // 下載圖片
        using (var webClient = new System.Net.WebClient())
        {
            byte[] imageBytes = webClient.DownloadData(imageSrc);
            using (var ms = new System.IO.MemoryStream(imageBytes))
            {
                // 使用 Tesseract OCR 辨識文字
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromMemory(ms.ToArray()))
                    {
                        var result = engine.Process(img);
                        return result.GetText().Trim();
                    }
                }
            }
        }
    }

    public static void PayTicket(IWebDriver driver, string cardNumber, string cardMonth, string cardYear)
    {
        while (true)
        {
            if (driver.Url == "https://epos.cloud.cathaybk.com.tw/EPOSPayment/#/paymentProcessing/orderpayment")
            {
                // 卡號
                driver.FindElement(By.Id("cardNumber")).SendKeys(cardNumber);
                var monthSelect = driver.FindElement(By.Id("ExpirationMonth"));
                monthSelect.FindElement(By.CssSelector($"option[value='{cardMonth}']")).Click();

                var yearSelect = driver.FindElement(By.Id("ExpirationYear"));
                yearSelect.FindElement(By.CssSelector($"option[value='{cardYear}']")).Click();

                driver.FindElement(By.Id("check_num")).SendKeys("695");
            }

            Thread.Sleep(1000); // 每秒檢查一次
        }
    }
}

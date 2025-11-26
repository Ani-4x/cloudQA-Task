using System;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace CloudQATests
{
    [TestFixture]
    public class AutomationPracticeFormTests
    {
        private IWebDriver driver;

        [SetUp]
        public void SetUp()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");

            driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl("https://app.cloudqa.io/home/AutomationPracticeForm");
        }

        [TearDown]
        public void TearDown()
        {
            Thread.Sleep(4000);

            if (driver != null)
            {
                try
                {
                    driver.Quit();
                }
                catch { }
                try
                {
                    driver.Dispose();
                }
                catch { }
            }
        }

        /// <summary>
        /// First Name
        /// </summary>
        private IWebElement GetFieldByLabel(string labelText)
        {
            var xpath =
                $"//label[normalize-space()='{labelText}']"
                + "/following::*[self::input or self::textarea][1]";

            return driver.FindElement(By.XPath(xpath));
        }

        /// <summary>
        /// Country
        /// </summary>
        private IWebElement GetCountrySelect()
        {
            var xpath =
                "//label[normalize-space()='Country']/following::select[1]"
                + " | //span[normalize-space()='Country']/following::select[1]"
                + " | //div[.//*[normalize-space()='Country']]//select[1]";

            return driver.FindElement(By.XPath(xpath));
        }

        /// <summary>
        /// Hobbies (Checkboxes)
        /// </summary>
        private IWebElement GetHobbyOption(string hobbyText)
        {
            // Strategy 1: Try to find the input by its 'value' attribute (caseInsensitive)
            string valueXpath =
                $"//input[@type='checkbox' and (translate(@value, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '{hobbyText.ToLower()}')]";

            var elements = driver.FindElements(By.XPath(valueXpath));
            if (elements.Count > 0)
            {
                return elements[0];
            }

            // Strategy 2: If 'value' fails, find the exact text node (not a container) and find the nearest preceding checkbox.
            string textXpath =
                $"//input[@type='checkbox'][following::text()[contains(., '{hobbyText}')]][last()]";

            // fallBack: Sometimes the input is INSIDE the label
            string nestedXpath =
                $"//label[contains(text(), '{hobbyText}')]//input[@type='checkbox']";

            try
            {
                return driver.FindElement(By.XPath(textXpath));
            }
            catch (NoSuchElementException)
            {
                return driver.FindElement(By.XPath(nestedXpath));
            }
        }

        [Test]
        public void FillForm_ShouldEnterDataSequentially()
        {
            // First Name
            string firstName = "Animesh";
            var firstNameInput = GetFieldByLabel("First Name");
            firstNameInput.Clear();
            firstNameInput.SendKeys(firstName);

            Thread.Sleep(1000);

            string actualName = firstNameInput.GetAttribute("value") ?? string.Empty;
            Assert.That(
                actualName,
                Is.EqualTo(firstName),
                "First Name field does not contain the expected value."
            );

            // Last Name
            string lastName = "Gawande";
            var lastNameInput = GetFieldByLabel("Last Name");
            lastNameInput.Clear();
            lastNameInput.SendKeys(lastName);

            Thread.Sleep(1000);

            string actualLastName = lastNameInput.GetAttribute("value") ?? string.Empty;
            Assert.That(
                actualLastName,
                Is.EqualTo(lastName),
                "Last Name field does not contain the expected value."
            );

            // Hobbies
            string[] Hobbies = { "Dance", "Cricket" }; // can select any combination

            foreach (var hobby in Hobbies)
            {
                var hobbyCheckbox = GetHobbyOption(hobby);

                if (!hobbyCheckbox.Selected)
                {
                    hobbyCheckbox.Click();
                }

                Thread.Sleep(500);

                Assert.That(hobbyCheckbox.Selected, Is.True, $"Hobby '{hobby}' was not selected.");
            }

            // Country
            const string countryToSelect = "India";
            var countrySelect = GetCountrySelect();
            var selectElement = new SelectElement(countrySelect);

            selectElement.SelectByText(countryToSelect);

            Thread.Sleep(1000);

            var selectedText = selectElement.SelectedOption.Text.Trim();
            Assert.That(
                selectedText,
                Is.EqualTo(countryToSelect),
                "The selected country is not the expected value."
            );
        }
    }
}

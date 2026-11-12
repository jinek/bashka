using System.Text.RegularExpressions;
using Avalonia.Threading;
using Consolonia.NUnit;

namespace Bashka.Tests.Helpers;

internal static class UITestHelper
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);

    extension(UnitTestConsole test)
    {
        public async Task WaitText(string regex)
        {
            await WaitForMatch(test, regex, true);
        }

        public async Task WaitNoText(string regex)
        {
            await WaitForMatch(test, regex, false);
        }
    }

    private static async Task WaitForMatch(UnitTestConsole test, string regex, bool shouldMatch)
    {
        DateTime deadline = DateTime.UtcNow + Timeout;
        var compiledRegex = new Regex(regex);

        while (true)
        {
            string lastBuffer = await Dispatcher.UIThread.InvokeAsync(() => test.PixelBuffer.PrintBuffer());
            bool isMatch = compiledRegex.IsMatch(lastBuffer);
            if (isMatch == shouldMatch)
                return;

            if (DateTime.UtcNow >= deadline)
            {
                string message = shouldMatch
                    ? $"Text '{regex}' was not found in the buffer within {Timeout}: \r\n" + lastBuffer
                    : $"Text '{regex}' was still found in the buffer within {Timeout}: \r\n" + lastBuffer;
                Assert.Fail(message);
                return;
            }

            await Task.Delay(200);
        }
    }
}

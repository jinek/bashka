using System.Text.RegularExpressions;
using Avalonia.Threading;
using Consolonia.Core.Drawing.PixelBufferImplementation;
using Consolonia.NUnit;

namespace Bashka.Tests.Helpers;

internal static class UITestHelper
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);

    extension(UnitTestConsole test)
    {
        public async Task WaitBuffer(Func<PixelBuffer, bool> condition, string description)
        {
            DateTime deadline = DateTime.UtcNow + Timeout;

            while (true)
            {
                (bool isMet, string lastBuffer) = await Dispatcher.UIThread.InvokeAsync(() =>
                    (condition(test.PixelBuffer), test.PixelBuffer.PrintBuffer()));
                if (isMet)
                    return;

                if (DateTime.UtcNow >= deadline)
                {
                    Assert.Fail($"{description} within {Timeout}: \r\n" + lastBuffer);
                    return;
                }

                await Task.Delay(200);
            }
        }

        public async Task WaitText(string regex)
        {
            var compiledRegex = new Regex(regex);
            await test.WaitBuffer(buffer => compiledRegex.IsMatch(buffer.PrintBuffer()),
                $"Text '{regex}' was not found in the buffer");
        }

        public async Task WaitNoText(string regex)
        {
            var compiledRegex = new Regex(regex);
            await test.WaitBuffer(buffer => !compiledRegex.IsMatch(buffer.PrintBuffer()),
                $"Text '{regex}' was still found in the buffer");
        }
    }
}
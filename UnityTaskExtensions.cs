using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UniAgile.Unity
{
    public static class UnityTaskExtensions
    {

        public static async Task WaitFoCompletion(this AsyncOperation operation,
                                                  CancellationToken cancellationToken)
        {
            await Task.WhenAny(TaskEx.WaitUntil(operation, (op) => op.isDone, true), 
                               cancellationToken.RunUntilCancellation());
        }


        public static async Task<Button> WaitForAnyButtonClick(Button button1,
                                                               Button button2,
                                                               Button button3,
                                                               CancellationToken cancellationToken)
        {
            // you need to do this, otherwise things break
            var result = await Task.WhenAny(button1.WaitForClick(cancellationToken),
                                      button2.WaitForClick(cancellationToken),
                                      button3.WaitForClick(cancellationToken), 
                                      cancellationToken.RunUntilCancellation<Button>())
                             .Result;

            return result;
        }

        public static async Task<Button> WaitForAnyButtonClick(Button button1,
                                                               Button button2,
                                                               CancellationToken cancellationToken)
        {
            // you need to do this, otherwise things break
            var result = await Task.WhenAny(
                                       button1.WaitForClick(cancellationToken),
                                      button2.WaitForClick(cancellationToken),
                                       cancellationToken.RunUntilCancellation<Button>());

            return result.Result;
        }

        public static async Task<T> WaitForAnyButtonClick<T>((Button Button, T Retval) first,
                                                             (Button Button, T Retval) second,
                                                             (Button Button, T Retval) third,
                                                             CancellationToken cancellationToken)
        {
            var res = await Task.WhenAny(first.Button.WaitForClick(cancellationToken),
                                            second.Button.WaitForClick(cancellationToken),
                                            third.Button.WaitForClick(cancellationToken), 
                                            cancellationToken.RunUntilCancellation<Button>());

            var result = res.Result;

            if (result == first.Button)
            {
                return first.Retval;
            }

            if (result == second.Button)
            {
                return second.Retval;
            }

            return third.Retval;
        }

        public static async Task<T> WaitForAnyButtonClick<T>((Button Button, T Retval) first,
                                                             (Button Button, T Retval) second,
                                                             CancellationToken cancellationToken)
        {
            var res = await Task.WhenAny(first.Button.WaitForClick(cancellationToken),
                                            second.Button.WaitForClick(cancellationToken),
                                            cancellationToken.RunUntilCancellation<Button>());

            var result = res.Result;

            return result == first.Button ? first.Retval : second.Retval;
        }

        private class ButtonAwaiter
        {
            private Button Button;
            private bool IsPressed;

            public ButtonAwaiter(Button button)
            {
                Button = button;
                IsPressed = false;
            }

            private void OnButtonClicked()
            {
                IsPressed = true;
            }
            
            public async Task<Button> WaitUntilPressed(CancellationToken cancellationToken)
            {
                Button.onClick.AddListener(OnButtonClicked);

                while (!IsPressed)
                {
                    await Task.Yield();
                }
                
                Button.onClick.RemoveListener(OnButtonClicked);


                return Button;
                
            }
            
            
        }

        public static async Task<Button> WaitForClick(this Button button,
                                                               CancellationToken cancellationToken)
        {
            // avoids garbage
            var awaiter = new ButtonAwaiter(button);

            await awaiter.WaitUntilPressed(cancellationToken);
            
            GC.SuppressFinalize(awaiter);
            
            return cancellationToken.IsCancellationRequested ? default : button;

        }
    }
}
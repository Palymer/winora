namespace WindowsOptimizer.Infrastructure.Native;

internal static class ComApartment
{
    public static T Run<T>(Func<T> action)
    {
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        {
            return action();
        }

        T? result = default;
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        thread.Join();
        if (error is not null)
        {
            throw error;
        }

        return result!;
    }

    public static void Run(Action action) =>
        Run<object?>(() =>
        {
            action();
            return null;
        });
}

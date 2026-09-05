using System.Diagnostics;

namespace SI_300D.Services
{
    public class ProcessService
    {
        public string GetProcessName(uint processId)
        {
            if (processId == 0)
                return string.Empty;

            try
            {
                using var process = Process.GetProcessById((int)processId);

                return process.ProcessName;
            }
            catch (ArgumentException)
            {
                // Process no longer exists.
                return string.Empty;
            }
            catch (InvalidOperationException)
            {
                // Process has already exited.
                return string.Empty;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Access denied or process information unavailable.
                return string.Empty;
            }
        }
    }
}
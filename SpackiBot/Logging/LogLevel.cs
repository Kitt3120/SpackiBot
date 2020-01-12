using System;
using System.Collections.Generic;
using System.Text;

namespace SpackiBot.Logging
{
    enum LogLevel
    {
        //
        // Zusammenfassung:
        //     Logs that contain the most severe level of error. This type of error indicate
        //     that immediate attention may be required.
        Critical = 0,

        //
        // Zusammenfassung:
        //     Logs that highlight when the flow of execution is stopped due to a failure.
        Error = 1,

        //
        // Zusammenfassung:
        //     Logs that highlight an abnormal activity in the flow of execution.
        Warning = 2,

        //
        // Zusammenfassung:
        //     Logs that track the general flow of the application.
        Info = 3,

        //
        // Zusammenfassung:
        //     Logs that are used for interactive investigation during development.
        Verbose = 4,

        //
        // Zusammenfassung:
        //     Logs that contain the most detailed messages.
        Debug = 5
    }
}

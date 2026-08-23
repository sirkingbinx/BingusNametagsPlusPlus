using System;
using BingusNametagsPlusPlus.Utilities;

namespace BingusNametagsPlusPlus;

public static class Extensions
{
    public static void Report(this Exception ex) => LogManager.LogException(ex);
}
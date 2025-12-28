using System;

string str = "ACTION: CUSTOM ACTION MyCustomAction";
int idx = str.ToUpper().IndexOf("custom action ");
Console.WriteLine($"Index: {idx}");
if (idx != -1)
{
    string extracted = str.Substring(idx + 14);
    Console.WriteLine($"Extracted: {extracted}");
}

using MyCOMComponent;
using System;
using System.Runtime.InteropServices;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[Guid("A17E5617-CC14-4E3F-B686-A6D2D813BE1A")]
public class ExpressImpl : IExpress
{
    public string minus(int a, int b)
    {
        return $"{a - b} = {a} - {b}";
    }

    public string divide(int a, int b)
    {
        if (b == 0)
        {
            return "除零错误";
        }
        return $"{a / b} = {a} / {b}";
    }
}

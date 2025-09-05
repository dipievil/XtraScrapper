using System.Resources;
using System.Globalization;

namespace XtraRCleaner;

public class SimpleLocalizer
{
    private readonly ResourceManager _resourceManager;
    
    public SimpleLocalizer()
    {
        _resourceManager = new ResourceManager("XtraRCleaner.Resources.Resources", typeof(SimpleLocalizer).Assembly);
    }
    
    public string this[string key] => _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
    
    public string this[string key, params object[] args] 
    {
        get
        {
            var format = _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
            return string.Format(format, args);
        }
    }
}

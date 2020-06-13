using Microsoft.Maps.MapControl.WPF;

namespace OpenDataDWD.Model
{
    /// <summary>
    /// PushPin class with an added ID 
    /// </summary>
    public class PushPinWithID : Pushpin
    {
        public int Id { get; set; }
    }
}

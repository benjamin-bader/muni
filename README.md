# Muni
## Helping your data arrive on time

Muni is a simple message bus for Windows Phone and Xamarin.iOS, and Xamarin.Android.

Sending events is the height of simplicity:

```csharp
public class SyncManager
{
    private Bus bus = /* insert code here */

    public void Sync()
    {
        // code code code
	var result = new SyncData(bytes);
	bus.Post(result); // send a SyncData message
    }
}
```

Receiving events is as easy as adding a `[Subscribe]` attribute to a single-parameter method:

```csharp
public class MyViewModel
{
    private readonly Bus bus;

    public MyViewModel(Bus bus)
    {
        this.bus = bus;
	bus.Register(this);
    }

    // This method will be invoked whenever a SyncData message is posted
    [Subscribe]
    public void OnSyncComplete(SyncData data)
    {
        Console.WriteLine("Data synced: {0}", data);
    }
}
```

By default, Muni requires that all messages be posted to the UI thread, encouraging a more mindful design.  Should you decide that you don't need to care about what thread handles messages, you can create a `Bus` with an appropriate ThreadEnforcer:

```csharp
var myBus = new Bus(ThreadEnforcer.Any);
```



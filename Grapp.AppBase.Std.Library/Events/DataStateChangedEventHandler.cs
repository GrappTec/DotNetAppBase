namespace Grapp.AppBase.Std.Library.Events
{
	public delegate void DataStateChangedEventHandler<TData, TState>(object sender, DataStateChangedEventArgs<TData, TState> e);
}
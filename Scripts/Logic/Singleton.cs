using Godot;
using System;

public partial class Singleton<T> : Node 
	where T : Node {

	private static Node _instance;
	public static T instance => (T)_instance;
	
	public sealed override void _Ready() {
	    _instance = this;
	    _SingletonReady();
	}

	public virtual void _SingletonReady() { }

}

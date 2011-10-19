# InjectedXna
InjectedXna is a POC project for drawing in WoW using the XNA framework. The `InjectedGame` class attempts to mimic the XNA `Game` class, but uses the DirectX Device pointer of WoW for drawing. 
Pretty hacky, not complete, and liable to break. 
See *samples/Primitive3DSample.sln* for an example of using it with one of the default XNA samples. 
View commit history on *samples/Primitive3DSample/Primitives3D/Primitive3DGame.cs* to see how it was implemented.
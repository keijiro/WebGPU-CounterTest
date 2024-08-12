using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Unity.Collections;
using System.Linq;

public sealed class Test : MonoBehaviour
{
    [field:SerializeField] ComputeShader Compute { get; set; }

    GraphicsBuffer _countBuffer;
    GraphicsBuffer _valueBuffer;
    NativeArray<uint> _readBuffer;

    const int EntryCount = 4;

    void Start()
    {
        _countBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Counter | GraphicsBuffer.Target.Structured, 1, 4);
        _valueBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, EntryCount, 4);
        _readBuffer = new NativeArray<uint>(EntryCount, Allocator.Persistent);
    }

    void OnDestroy()
    {
        _countBuffer.Release();
        _valueBuffer.Release();
        _readBuffer.Dispose();
    }

    void Update()
    {
        _countBuffer.SetCounterValue(0);

        Compute.SetBuffer(0, "ValueBuffer", _valueBuffer);
        Compute.SetBuffer(0, "CountBuffer", _countBuffer);
        Compute.Dispatch(0, EntryCount, 1, 1);

        AsyncGPUReadback.RequestIntoNativeArray(ref _readBuffer, _valueBuffer);
        AsyncGPUReadback.WaitAllRequests();

        var label = GetComponent<UIDocument>().rootVisualElement.Q<Label>();
        label.text = string.Join(",", _readBuffer.ToArray());
    }
}

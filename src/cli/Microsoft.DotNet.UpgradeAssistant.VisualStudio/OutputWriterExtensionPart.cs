// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.Editor.UI;

namespace Microsoft.DotNet.UpgradeAssistant.VisualStudio;

[ExtensionPart(typeof(ITextViewLifetimeListener))]
[ExtensionPart(typeof(ITextViewChangedListener))]
[AppliesTo(ContentType = "CSharp")]
public sealed class OutputWriterExtensionPart : ExtensionPart, ITextViewLifetimeListener, ITextViewChangedListener
{
    private readonly IOutputWindowWriter _writer;

    public OutputWriterExtensionPart(ExtensionCore container, VisualStudioExtensibility extensibilityObject, IOutputWindowWriter writer)
        : base(container, extensibilityObject)
    {
        _writer = writer;
    }

    public Task TextViewChangedAsync(TextViewChangedArgs args, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task TextViewClosedAsync(ITextView textView, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task TextViewCreatedAsync(ITextView textView, CancellationToken cancellationToken)
        => Task.CompletedTask;

    protected override Task InitializeAsync(CancellationToken cancellationToken)
        => _writer.DrainAsync(Extensibility, cancellationToken);
}

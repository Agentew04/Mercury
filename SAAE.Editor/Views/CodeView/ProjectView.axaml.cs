using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Models;
using SAAE.Editor.ViewModels;
using ProjectViewModel = SAAE.Editor.ViewModels.Code.ProjectViewModel;

namespace SAAE.Editor.Views.CodeView;

public partial class ProjectView : UserControl {

    private readonly ILogger<ProjectView> logger = App.Services.GetRequiredService<ILogger<ProjectView>>();
    private TopLevel? topLevel;
    
    public ProjectView() {
        InitializeComponent();
        DataContext = ViewModel = App.Services.GetRequiredService<ProjectViewModel>();
        AddHandler(DragDrop.DragOverEvent, DragOver);
        AddHandler(DragDrop.DropEvent, Drop);
    }
    
    public ProjectViewModel ViewModel { get; set; }

    private Point ghostPosition = new(0,0);
    private readonly Point mouseOffset = new(-5, -5);
    private bool pressed = false;
    private TaskCompletionSource? moveCompletionSource;

    protected override void OnLoaded(RoutedEventArgs e) {
        GhostBorder.IsVisible = false;
        topLevel = TopLevel.GetTopLevel(this)!;
        Debug.Assert(topLevel != null);
        base.OnLoaded(e);
    }

    private async void ProjectNode_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
        ProjectNode? node = (ProjectNode?)((InputElement?)sender)?.DataContext;
        if (node is null) {
            return;
        }
        if (e.ClickCount == 2) {
            ViewModel.SelectNode(node);
            return;
        }

        if (node.IsEffectiveReadOnly || node.Type == ProjectNodeType.Category
            || ViewModel.IsEntryPoint(node)) {
            return;
        }

        pressed = true;
        moveCompletionSource = new TaskCompletionSource();
        try {
            await moveCompletionSource.Task;
        }
        catch (TaskCanceledException) {
            moveCompletionSource = null;
            return;
        }
        moveCompletionSource = null;
        
        logger.LogInformation("Drag Start");
        Point ghostPos = GhostBorder.Bounds.Position;
        ghostPosition = ghostPos + mouseOffset;
        
        Point mousePos = e.GetPosition(topLevel);
        double offsetX = mousePos.X - ghostPos.X;
        double offsetY = mousePos.Y - ghostPos.Y + mouseOffset.X;
        GhostBorder.RenderTransform = new TranslateTransform(offsetX, offsetY);

        ViewModel.StartDrag(node);
        GhostTextBlock.Text = node.Name;
        //GhostBorder.IsVisible = true;
        var dragData = new DataObject();
        dragData.Set(nameof(ProjectNode), node);
        try {
            DragDropEffects result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
        }
        catch (COMException) {
            logger.LogError("COMException. Usuario tentou arrastar pasta do jeito errado. ");
        }

        GhostBorder.IsVisible = false;
        ViewModel.ForceEndDrag();
    }

    private void DragOver(object? sender, DragEventArgs e) {
        Point currentPosition = e.GetPosition(topLevel!);
        Point offset = currentPosition - ghostPosition;
        GhostBorder.RenderTransform = new TranslateTransform(offset.X, offset.Y);

        e.DragEffects = DragDropEffects.Move;
        ProjectNode? target = (ProjectNode?)(e.Source as InputElement)?.DataContext;
        if (target is not null && ViewModel.IsNodeValidForDrop(target)) return;
        e.DragEffects = DragDropEffects.None;
    }

    private void Drop(object? sender, DragEventArgs e) {
        ProjectNode? node = (ProjectNode?)e.Data.Get(nameof(ProjectNode));
        ProjectNode? target = (ProjectNode?)(e.Source as InputElement)?.DataContext;
        if (node is null || target is null) {
            return;
        }
        pressed = false;
        ViewModel.Drop(node, target);
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e) {
        moveCompletionSource?.SetResult();
    }

    private void InputElement_OnPointerExited(object? sender, PointerEventArgs e) {
        if (pressed) {
            moveCompletionSource?.SetCanceled();
            pressed = false;
        }
    }

    private void ProjectNode_OnPointerReleased(object? sender, PointerReleasedEventArgs e) {
        moveCompletionSource?.SetCanceled();
        pressed = false;
    }
}
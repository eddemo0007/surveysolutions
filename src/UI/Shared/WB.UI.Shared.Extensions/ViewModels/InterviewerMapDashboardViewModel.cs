﻿using System.Drawing;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Extensions.Services;
using WB.UI.Shared.Extensions.ViewModels.Markers;

namespace WB.UI.Shared.Extensions.ViewModels;

public class InterviewerMapDashboardViewModel : MapDashboardViewModel
{
    protected override InterviewStatus[] InterviewStatuses { get; } = 
    {
        InterviewStatus.Created,
        InterviewStatus.InterviewerAssigned,
        InterviewStatus.Restarted,
        InterviewStatus.RejectedBySupervisor,
        InterviewStatus.Completed,
    };

    public InterviewerMapDashboardViewModel(IPrincipal principal, 
        IViewModelNavigationService viewModelNavigationService, 
        IUserInteractionService userInteractionService, 
        IMapService mapService, 
        IAssignmentDocumentsStorage assignmentsRepository, 
        IPlainStorage<InterviewView> interviewViewRepository, 
        IEnumeratorSettings enumeratorSettings, 
        ILogger logger, 
        IMapUtilityService mapUtilityService, 
        IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher 
        ) 
        : base(principal, viewModelNavigationService, userInteractionService, mapService, assignmentsRepository, interviewViewRepository, enumeratorSettings, logger, mapUtilityService, mainThreadAsyncDispatcher)
    {
    }

    public override bool SupportDifferentResponsible => false;

    protected override IInterviewMarkerViewModel GetInterviewMarkerViewModel(InterviewView interview)
    {
        return new InterviewerInterviewMarkerViewModel(interview);
    }

    protected override Symbol GetInterviewMarkerSymbol(IInterviewMarkerViewModel interview)
    {
        Color markerColor;

        switch (interview.Status)
        {
            case InterviewStatus.Created:
            case InterviewStatus.InterviewerAssigned:
            case InterviewStatus.Restarted:    
                markerColor = Color.FromArgb(0x2a, 0x81, 0xcb);
                break;
            case InterviewStatus.Completed:
                markerColor = Color.FromArgb(0x1f,0x95,0x00);
                break;
            case InterviewStatus.RejectedBySupervisor:
                markerColor = Color.FromArgb(0xe4,0x51,0x2b);
                break;
            default:
                markerColor = Color.Yellow;
                break;
        }

        return new CompositeSymbol(new[]
        {
            new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.White, 22), //for contrast
            new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, markerColor, 16)
        });
    }

    protected override IAssignmentMarkerViewModel GetAssignmentMarkerViewModel(AssignmentDocument assignment)
    {
        return new InterviewerAssignmentMarkerViewModel(assignment);
    }
}

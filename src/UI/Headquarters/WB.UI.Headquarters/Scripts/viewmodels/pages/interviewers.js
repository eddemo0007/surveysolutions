﻿Supervisor.VM.Interviewers = function (listViewUrl, archiveUsersUrl, ajax, interviewersPageUrl, supervisorsUrl) {
    Supervisor.VM.Interviewers.superclass.constructor.apply(this, [listViewUrl, archiveUsersUrl, ajax]);

    var self = this;

    self.Url = new Url(interviewersPageUrl);

    self.SupervisorUrl = supervisorsUrl;
    self.IsSupervisorsLoading = ko.observable(false);
    self.Supervisors = function (query, sync, pageSize) {
        self.IsSupervisorsLoading(true);
        self.SendRequest(self.SupervisorUrl, { query: query, pageSize: pageSize, showLocked: true }, function (response) {
            sync(response.Users, response.TotalCountByQuery);
        }, true, true, function () {
            self.IsSupervisorsLoading(false);
        });
    }
    self.SelectedSupervisor = ko.observable();

    self.Archived = ko.observable(false);
    self.Facet = ko.observable();

    self.GetFilterMethod = function () {

        return {
            SupervisorName : _.isUndefined(self.SelectedSupervisor())
                                ? null
                                : self.SelectedSupervisor().UserName,
            Archived : self.Archived(),
            Facet: self.Facet()
        }
    };

    self.load = function () {

        if (self.QueryString['supervisor'] != null) {
            self.SelectedSupervisor({ UserName: self.QueryString['supervisor'] });
        }

        self.Archived(self.QueryString['archived']);
        self.Facet(self.QueryString['Facet'] || null);

        self.Url.query['supervisor'] = self.QueryString['supervisor'] || "";
        self.Url.query['archived'] = self.QueryString['archived'] || "";
        self.Url.query['Facet'] = self.QueryString['Facet'] || "";

        setTimeout(function() {
            $('.facet').selectpicker('val', self.Facet());
        }, 300);

        self.SelectedSupervisor.subscribe(self.reloadDataTable);
        self.Archived.subscribe(self.reloadDataTable);
        self.Facet.subscribe(self.reloadDataTable);
        
        self.initDataTable(this.onDataTableDataReceived, this.onTableInitComplete);
    }
};

Supervisor.Framework.Classes.inherit(Supervisor.VM.Interviewers, Supervisor.VM.EditableUsers);

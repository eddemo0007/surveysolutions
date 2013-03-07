﻿define('model.group',
    ['ko', 'config'],
    function(ko, config) {

        var _dc = null,
            Group = function() {
                var self = this;
                self.id = ko.observable();
                self.title = ko.observable();
                self.type = ko.observable();
                self.level = ko.observable();
                self.description = ko.observable();
                self.condition = ko.observable();
                
                self.children = ko.observableArray();
                self.childrenID = ko.observableArray();
                self.template = "GroupView";
                self.getHref = function () {
                    return config.hashes.detailsGroup + "/" + self.id();
                };

                self.typeOptions = config.groupTypes;
                self.isNullo = false;
                self.dirtyFlag = new ko.DirtyFlag([self.title, self.type]);
                return self;
            };
        
        Group.datacontext = function(dc) {
            if (dc) {
                _dc = dc;
            }
            return _dc;
        };

        Group.prototype = function() {
            var dc = Group.datacontext,
                fillChildren = function () {
                     var items =_.map(this.childrenID(), function (item) {
                        if (item.type === "GroupView")
                            return dc().groups.getLocalById(item.id);
                        return dc().questions.getLocalById(item.id);
                     });
                    this.children(items);
                    //return self.children();
                };
            return {
                isNullo: false,
                fillChildren: fillChildren
            };
        }();

        Group.Nullo = new Group().id(0).title('Title').type('GroupView');
        Group.Nullo.isNullo = true;
        Group.Nullo.dirtyFlag().reset();

        return Group;
    });

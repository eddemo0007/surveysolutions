﻿define('model.group',
    ['ko', 'config','utils'],
    function (ko, config, utils) {

        var _dc = null,
            Group = function() {
                var self = this;
                self.id = ko.observable(Math.uuid());
                self.isNew = ko.observable(true);
                self.isClone = ko.observable(false);
                
                self.title = ko.observable('New Group').extend({ required: true });
                self.parent = ko.observable();

                self.type = ko.observable("GroupView"); // Object type
                self.template = "GroupView"; // inner html template name
                self.gtype = ko.observable("None"); // Group type

                self.level = ko.observable();
                self.description = ko.observable('');
                self.condition = ko.observable('').extend({
                    validation: [{
                        validator: function (val) {
                            return (val.indexOf("[this]") == -1);
                        },
                        message: 'You cannot use self-reference in conditions'
                    }]
                });;
                
                self.children = ko.observableArray();
                self.childrenID = ko.observableArray();
                
                // UI stuff
                self.tip = ko.computed(function () {
                    if (self.isNew()) return config.tips.newGroup;
                    return null;
                });
                
                self.getHref = function () {
                    return config.hashes.detailsGroup + "/" + self.id();
                };

                self.cloneSource = ko.observable();
                self.isSelected = ko.observable();
                self.isExpanded = ko.observable(true);
                self.typeOptions = config.groupTypes;
                self.isNullo = false;
                self.dirtyFlag = new ko.DirtyFlag([self.title, self.gtype, self.description, self.condition]);
                self.dirtyFlag().reset();
                
                self.errors = ko.validation.group(self);
                
                self.canUpdate = ko.observable(true);
                this.cache = function () { };
                return self;
            };
        
        Group.datacontext = function(dc) {
            if (dc) {
                _dc = dc;
            }
            return _dc;
        };

        var BaseGroup = function() {
            var dc = Group.datacontext,
                index = function () {
                    if (this.hasParent()) {
                        var parent = this.parent();
                        var item = utils.findById(parent.childrenID(), this.id());
                        return item.index;
                    }
                    return 0;
                },
                hasParent = function () {
                    if (_.isNull(this.parent()) || _.isUndefined(this.parent())) {
                        return false;
                    }
                    return true;
                },
                fillChildren = function () {
                     var items =_.map(this.childrenID(), function (item) {
                        if (item.type === "GroupView")
                            return dc().groups.getLocalById(item.id);
                        return dc().questions.getLocalById(item.id);
                     });
                     this.children(items);
                     this.children.id = this.id();
                    //return self.children();
                },
                clone = function () {
                    var item = new Group();
                    item.title(this.title());
                    item.type(this.type());
                    item.gtype(this.gtype());
                    item.condition(this.condition());
                    item.level(this.level());
                    item.description(this.description());
                    item.parent(this.parent());
                    item.id(Math.uuid());
                    item.isNew(true);
                    item.isClone(true);

                    item.childrenID(_.map(this.childrenID(), function (child) {
                        var clonedItem;
                        
                        if (child.type === "GroupView") {
                            clonedItem = dc().groups.getLocalById(child.id).clone();
                            dc().groups.add(clonedItem);
                        } else {
                            clonedItem = dc().questions.getLocalById(child.id).clone();
                            dc().questions.add(clonedItem);
                        }
                        clonedItem.parent(item);
                        return { type: clonedItem.type(), id: clonedItem.id() };
                    }));

                    if (this.isClone() && this.isNew()) {
                        item.cloneSource(this.cloneSource());
                    } else {
                        item.cloneSource(this);
                    }

                    item.dirtyFlag().reset();
                    item.fillChildren();
                    return item;
                };;
            return {
                isNullo: false,
                fillChildren: fillChildren,
                index: index,
                hasParent: hasParent,
                clone: clone
            };
        };


        Group.prototype = new BaseGroup();

        Group.Nullo = new Group().id(0).title('Title').type('GroupView');
        Group.Nullo.isNullo = true;
        Group.Nullo.dirtyFlag().reset();

        ko.utils.extend(Group.prototype, {
            update: function (data) {
                
                this.title(data.title);
                this.gtype(data.gtype);
                this.description(data.description);
                this.condition(data.condition);

                //save off the latest data for later use
                this.cache.latestData = data;
            },
            revert: function () {
                this.update(this.cache.latestData);
            },
            commit: function () {
                this.cache.latestData = ko.toJS(this);
            }
        });

        return Group;
    });

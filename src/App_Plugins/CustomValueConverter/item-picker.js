/* Copyright © 2019 Lee Kelleher.
 * This Source Code has been derived from Contentment.
 * https://github.com/leekelleher/umbraco-contentment/blob/3.2.0/src/Umbraco.Community.Contentment/DataEditors/ItemPicker/item-picker.js
 * https://github.com/leekelleher/umbraco-contentment/blob/3.2.0/src/Umbraco.Community.Contentment/DataEditors/ItemPicker/item-picker.overlay.js
 * https://github.com/leekelleher/umbraco-contentment/blob/3.2.0/src/Umbraco.Community.Contentment/DataEditors/_/_components.js#L6-L149
 * Modified under the permissions of the Mozilla Public License.
 * Copyright © 2022 Lee Kelleher.
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

angular.module("umbraco").controller("Umbraco.Community.CustomValueConverter.ItemPicker.Controller", [
    "$scope",
    "editorService",
    "focusService",
    "localizationService",
    "overlayService",
    function ($scope, editorService, focusService, localizationService, overlayService) {

        // console.log("item-picker.model", $scope.model);

        var defaultConfig = {
            confirmRemoval: 1,
            defaultIcon: "icon-science",
            defaultValue: [],
            disableSorting: 1,
            enableFilter: 1,
            items: [],
            maxItems: 1,
            overlayView: "",
            overlayOrderBy: "name",
            overlaySize: "medium",
            addButtonLabelKey: "general_add",
        };
        var config = Object.assign({}, defaultConfig, $scope.model.config);

        var vm = this;

        function init() {

            $scope.model.value = $scope.model.value || config.defaultValue;

            if (Array.isArray($scope.model.value) === false) {
                $scope.model.value = [$scope.model.value];
            }

            if (Number.isInteger(config.maxItems) === false) {
                config.maxItems = Number.parseInt(config.maxItems) || defaultConfig.maxItems;
            }

            config.confirmRemoval = Object.toBoolean(config.confirmRemoval);

            vm.allowAdd = config.maxItems === 0 || $scope.model.value.length < config.maxItems;
            vm.allowEdit = false;
            vm.allowRemove = true;
            vm.allowSort = Object.toBoolean(config.disableSorting) === false && config.maxItems !== 1;

            vm.sortableOptions = {
                axis: "y",
                containment: "parent",
                cursor: "move",
                disabled: vm.allowSort === false,
                opacity: 0.7,
                scroll: true,
                tolerance: "pointer",
                stop: function (e, ui) {
                    $scope.model.value = vm.items.map(function (x) { return x.value });
                    setDirty();
                }
            };

            vm.addButtonLabelKey = config.addButtonLabelKey || "general_add";

            if (vm.addButtonLabelKey) {
                localizationService.localize(vm.addButtonLabelKey).then(function (label) {
                    vm.addButtonLabel = label;
                });
            }

            vm.add = add;
            vm.populate = populate;
            vm.remove = remove;

            vm.items = [];

            if ($scope.model.value.length > 0 && config.items.length > 0) {
                var orphaned = [];

                $scope.model.value.forEach(function (v) {
                    var item = config.items.find(x => x.value === v);
                    if (item) {
                        vm.items.push(Object.assign({}, item));
                    } else {
                        orphaned.push(v);
                    }
                });

                if (orphaned.length > 0) {
                    $scope.model.value = _.difference($scope.model.value, orphaned); // TODO: Replace Underscore.js dependency. [LK:2022-03-18]

                    if (config.maxItems === 0 || $scope.model.value.length < config.maxItems) {
                        vm.allowAdd = true;
                    }
                }
            }
        };

        function add() {

            focusService.rememberFocus();

            editorService.open({
                config: {
                    title: "Choose...",
                    enableFilter: Object.toBoolean(config.enableFilter),
                    defaultIcon: config.defaultIcon,
                    items: config.items,
                    orderBy: config.overlayOrderBy,
                    maxItems: config.maxItems === 0 ? config.maxItems : config.maxItems - vm.items.length
                },
                view: config.overlayView,
                size: config.overlaySize || "small",
                submit: function (selectedItems) {

                    // NOTE: Edge-case, if the value isn't set and the content is saved, the value becomes an empty string. ¯\_(ツ)_/¯
                    if (typeof $scope.model.value === "string") {
                        $scope.model.value = $scope.model.value.length > 0 ? [$scope.model.value] : config.defaultValue;
                    }

                    selectedItems.forEach(function (x) {
                        vm.items.push(angular.copy(x)); // TODO: Replace AngularJS dependency. [LK:2020-12-17]
                        $scope.model.value.push(x.value);
                    });

                    if (config.maxItems !== 0 && $scope.model.value.length >= config.maxItems) {
                        vm.allowAdd = false;
                    }

                    editorService.close();

                    setDirty();
                    setFocus();
                },
                close: function () {
                    editorService.close();
                    setFocus();
                }
            });
        };

        function populate(item, $index, propertyName) {
            switch (propertyName) {
                case "icon":
                    return item.icon || config.defaultIcon;
                default:
                    return item[propertyName];
            }
        };

        function remove($index) {

            focusService.rememberFocus();

            if (config.confirmRemoval === true) {
                var keys = ["contentment_removeItemMessage", "general_remove", "general_cancel", "contentment_removeItemButton"];
                localizationService.localizeMany(keys).then(function (data) {
                    overlayService.open({
                        title: data[1],
                        content: data[0],
                        closeButtonLabel: data[2],
                        submitButtonLabel: data[3],
                        submitButtonStyle: "danger",
                        submit: function () {
                            removeItem($index);
                            overlayService.close();
                        },
                        close: function () {
                            overlayService.close();
                            setFocus();
                        }
                    });
                });
            } else {
                removeItem($index);
            }
        };

        function removeItem($index) {

            vm.items.splice($index, 1);

            $scope.model.value.splice($index, 1);

            if (config.maxItems === 0 || $scope.model.value.length < config.maxItems) {
                vm.allowAdd = true;
            }

            setDirty();
        };

        function setDirty() {
            if ($scope.propertyForm) {
                $scope.propertyForm.$setDirty();
            }
        };

        function setFocus() {
            var lastKnownFocus = focusService.getLastKnownFocus();
            if (lastKnownFocus) {
                lastKnownFocus.focus();
            }
        };

        init();
    }
]);

angular.module("umbraco").controller("Umbraco.Community.CustomValueConverter.ItemPickerOverlay.Controller", [
    "$scope",
    function ($scope) {

        // console.log("item-picker-overlay.model", $scope.model);

        var defaultConfig = {
            title: "Select...",
            enableFilter: false,
            defaultIcon: "icon-science",
            items: [],
            orderBy: "name",
            maxItems: 0,
        };
        var config = Object.assign({}, defaultConfig, $scope.model.config);

        var vm = this;

        function init() {

            vm.title = config.title;
            vm.enableFilter = config.enableFilter;
            vm.defaultIcon = config.defaultIcon;
            vm.items = config.items;
            vm.orderBy = config.orderBy;

            vm.maxItems = config.maxItems;
            vm.itemCount = 0;
            vm.allowSubmit = false;

            vm.select = select;
            vm.submit = submit;
            vm.close = close;
        };

        function select(item) {

            if (item.disabled === true) {
                return;
            }

            $scope.model.value = item;

            submit();
        };

        function submit() {
            if ($scope.model.submit) {

                var selectedItems = [];

                selectedItems.push($scope.model.value);

                $scope.model.submit(selectedItems);
            }
        };

        function close() {
            if ($scope.model.close) {
                $scope.model.close();
            }
        };

        init();
    }
]);

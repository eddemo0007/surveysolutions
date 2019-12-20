<template>
    <HqLayout
        :title="title"
        :hasFilter="true"
        :topicButton="$t('Assignments.NewAssignment')"
        :topicButtonRef="config.isSupervisor ? null : config.api.surveySetup"
    >
        <Filters slot="filters">
            <FilterBlock
                :title="$t('Common.Questionnaire')"
                :tooltip="$t('Assignments.Tooltip_Filter_Questionnaire')"
            >
                <Typeahead
                    control-id="questionnaireId"
                    fuzzy
                    :placeholder="$t('Common.AllQuestionnaires')"
                    :value="questionnaireId"
                    :values="config.questionnaires"
                    v-on:selected="questionnaireSelected"
                />
            </FilterBlock>

            <FilterBlock
                :title="$t('Common.QuestionnaireVersion')"
                :tooltip="$t('Assignments.Tooltip_Filter_QuestionnaireVersion')"
            >
                <Typeahead
                    control-id="questionnaireVersion"
                    fuzzy
                    :placeholder="$t('Common.AllVersions')"
                    :values="questionnaireId == null ? null : questionnaireId.versions"
                    :value="questionnaireVersion"
                    :disabled="questionnaireId == null"
                    v-on:selected="questionnaireVersionSelected"
                />
            </FilterBlock>

            <FilterBlock
                :title="$t('Common.Responsible')"
                :tooltip="$t('Assignments.Tooltip_Filter_Responsible')"
            >
                <Typeahead
                    control-id="responsibleId"
                    :placeholder="$t('Common.AllResponsible')"
                    :value="responsibleId"
                    :ajax-params="responsibleParams"
                    v-on:selected="userSelected"
                    :fetch-url="config.api.responsible"
                ></Typeahead>
            </FilterBlock>

            <FilterBlock
                :title="$t('Assignments.ReceivedByTablet')"
                :tooltip="$t('Assignments.Tooltip_Filter_Received')"
            >
                <Typeahead
                    control-id="recieved-by-tablet"
                    noSearch
                    noClear
                    :values="ddlReceivedByTablet"
                    :value="receivedByTablet"
                    v-on:selected="receivedByTabletSelected"
                />
            </FilterBlock>

            <FilterBlock
                :title="$t('Assignments.ShowArchived')"
                :tooltip="$t('Assignments.Tooltip_Filter_ArchivedStatus')"
            >
                <Typeahead
                    control-id="show_archived"
                    noSearch
                    noClear
                    :values="ddlShowArchive"
                    :value="showArchive"
                    v-on:selected="showArchiveSelected"
                />
            </FilterBlock>
        </Filters>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            :addParamsToRequest="addParamsToRequest"
            :wrapperClass=" { 'table-wrapper': true }"
            @cell-clicked="cellClicked"
            @selectedRowsChanged="rows => selectedRows = rows"
            @totalRows="(rows) => totalRows = rows"
            @ajaxComlpete="isLoading = false"
            @page="resetSelection"
            :selectable="showSelectors"
        >
            <div
                class="panel panel-table"
                id="pnlAssignmentsContextActions"
                v-if="selectedRows.length"
            >
                <div class="panel-body">
                    <input class="double-checkbox-white" type="checkbox" checked disabled />
                    <label>
                        <span class="tick"></span>
                        {{ $t("Assignments.AssignmentsSelected", {count: selectedRows.length}) }}
                    </label>

                    <button
                        class="btn btn-lg btn-primary"
                        id="btnUnarchiveSelected"
                        v-if="showArchive.key && config.isHeadquarter"
                        @click="unarchiveSelected"
                    >{{ $t("Assignments.Unarchive") }}</button>

                    <button
                        class="btn btn-lg btn-primary"
                        id="btnAssignSelected"
                        v-if="!showArchive.key"
                        @click="assignSelected"
                    >{{ $t("Common.Assign") }}</button>

                    <button
                        class="btn btn-lg btn-warning"
                        id="btnCloseSelected"
                        v-if="config.isHeadquarter && !showArchive.key"
                        @click="closeSelected"
                    >{{ $t("Assignments.Close") }}</button>

                    <button
                        class="btn btn-lg btn-danger"
                        id="btnArchiveSelected"
                        v-if="!showArchive.key && config.isHeadquarter"
                        @click="archiveSelected"
                    >{{ $t("Assignments.Archive") }}</button>
                </div>
            </div>
        </DataTables>

        <ModalFrame ref="assignModal" :title="$t('Pages.ConfirmationNeededTitle')">
            <p>{{ $t("Assignments.NumberOfAssignmentsAffected", {count: selectedRows.length} )}}</p>
            <form onsubmit="return false;">
                <div class="form-group">
                    <label
                        class="control-label"
                        for="newResponsibleId"
                    >{{ $t("Assignments.SelectResponsible") }}</label>
                    <Typeahead
                        control-id="newResponsibleId"
                        :placeholder="$t('Common.Responsible')"
                        :value="newResponsibleId"
                        :ajax-params="{ }"
                        @selected="newResponsibleSelected"
                        :fetch-url="config.api.responsible"
                    ></Typeahead>
                </div>
                <div class="form-group">
                    <label class="control-label" for="commentsId">{{ $t("Assignments.Comments") }}</label>
                    <textarea
                        control-id="commentsId"
                        v-model="reassignComment"
                        :placeholder="$t('Assignments.EnterComments')"
                        name="comments"
                        rows="6"
                        maxlength="500"
                        class="form-control"
                    />
                </div>
            </form>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-primary"
                    @click="assign"
                    :disabled="!newResponsibleId"
                >{{ $t("Common.Assign") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                >{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>

        <ModalFrame ref="closeModal" :title="$t('Pages.ConfirmationNeededTitle')">
            <p v-if="selectedRows.length === 1">{{singleCloseMessage}}</p>
            <p v-else>{{ $t("Assignments.MultipleAssignmentsClose", {count: selectedRows.length} )}}</p>

            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-primary"
                    :disabled="isWebModeAssignmentSelected"
                    @click="close"
                >{{ $t("Assignments.Close") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                >{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>

        <ModalFrame
            ref="editAudioEnabledModal"
            :title="$t('Assignments.ChangeAudioRecordingModalTitle', {id: editedRowId} )"
        >
            <p>{{ $t("Assignments.AudioRecordingExplanation")}}</p>
            <form onsubmit="return false;">
                <div class="form-group">
                    <Checkbox
                        :label="$t('Assignments.AudioRecordingEnable')"
                        name="audioRecordingEnabled"
                        v-model="editedAudioRecordingEnabled"
                    />
                </div>
            </form>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-primary"
                    @click="upateAudioRecording"
                    :disabled="!showSelectors"
                >{{ $t("Common.Save") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                >{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>

        <ModalFrame
            ref="editQuantityModal"
            :title="$t('Assignments.ChangeSizeModalTitle', {assignmentId: editedRowId} )"
        >
            <p>{{ $t("Assignments.SizeExplanation")}}</p>
            <p v-if="!canEditQuantity">
                <b>{{ $t("Assignments.AssignmentSizeInWebMode")}}</b>
            </p>
            <form onsubmit="return false;">
                <div class="form-group" v-bind:class="{'has-error': errors.has('editedQuantity')}">
                    <label class="control-label" for="newQuantity">{{$t("Assignments.Size")}}</label>

                    <input
                        type="text"
                        class="form-control"
                        v-model.trim="editedQuantity"
                        name="editedQuantity"
                        v-validate="quantityValidations"
                        :data-vv-as="$t('Assignments.Size')"
                        maxlength="5"
                        autocomplete="off"
                        @keyup.enter="updateQuantity"
                        id="newQuantity"
                        placeholder="1"
                        :disabled="!canEditQuantity"
                    />

                    <p
                        v-for="error in errors.collect('editedQuantity')"
                        :key="error"
                        class="text-danger"
                    >{{error}}</p>
                </div>
            </form>
            <div class="modal-footer">
                <button
                    type="button"
                    class="btn btn-primary"
                    :disabled="!showSelectors || !canEditQuantity"
                    @click="updateQuantity"
                >{{$t("Common.Save")}}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                >{{$t("Common.Cancel")}}</button>
            </div>
        </ModalFrame>
    </HqLayout>
</template>

<script>
import * as toastr from 'toastr'

export default {
    data() {
        return {
            responsibleId: null,
            questionnaireId: null,
            questionnaireVersion: null,
            wasInitialized: false,
            responsibleParams: {showArchived: true, showLocked: true},
            questionnaireParams: {censusOnly: false},
            isLoading: false,
            selectedRows: [],
            totalRows: 0,
            showArchive: null,
            receivedByTablet: null,
            newResponsibleId: null,
            reassignComment: null,
            editedRowId: null,
            editedQuantity: null,
            editedAudioRecordingEnabled: null,
            canEditQuantity: null,
        }
    },

    computed: {
        isWebModeAssignmentSelected() {
            if (this.selectedRows.length !== 1) return false

            const data = this.$refs.table.table.rows({selected: true}).data()
            return data[0].webMode
        },
        singleCloseMessage() {
            if (this.isWebModeAssignmentSelected) {
                return this.$t('Assignments.AssignmentCloseWebMode', {
                    id: this.selectedRows[0],
                })
            }

            const dataRow = this.$refs.table.table.rows({selected: true}).data()[0]
            const result = this.$t('Assignments.SingleAssignmentCloseConfirm', {
                id: this.selectedRows[0],
                quantity: dataRow.quantity,
                collected: dataRow.interviewsCount,
            })
            return result
        },
        quantityValidations() {
            return {
                regex: '^-?([0-9]+)$',
                min_value: -1,
                max_value: this.config.maxInterviewsByAssignment,
            }
        },
        ddlReceivedByTablet() {
            return [
                {key: 'All', value: this.$t('Assignments.ReceivedByTablet_All')},
                {key: 'Received', value: this.$t('Assignments.ReceivedByTablet_Received')},
                {key: 'NotReceived', value: this.$t('Assignments.ReceivedByTablet_NotReceived')},
            ]
        },
        ddlShowArchive() {
            return [
                {key: false, value: this.$t('Assignments.Active')},
                {key: true, value: this.$t('Assignments.Archived')},
            ]
        },

        title() {
            return this.$t('Assignments.AssignmentsHeader') + ' (' + this.formatNumber(this.totalRows) + ')'
        },
        config() {
            return this.$config.model
        },

        questionnaireVersionFetchUrl() {
            if (this.questionnaireId && this.questionnaireId.key)
                return `${this.config.api.questionnaire}/${this.questionnaireId.key}`
            return null
        },

        tableOptionsraw() {
            const self = this

            return [
                {
                    data: 'id',
                    name: 'Id',
                    title: 'Id',
                    responsivePriority: 2,
                    render(data, type, row) {
                        var result = "<a href='Assignments/" + row.id + "'>" + data + '</a>'
                        return result
                    },
                },
                {
                    data: 'responsible',
                    name: 'Responsible.Name',
                    title: this.$t('Common.Responsible'),
                    tooltip: this.$t('Assignments.Tooltip_Table_Responsible'),
                    responsivePriority: 3,
                    render(data, type, row) {
                        var isInterviewerRole = row.responsibleRole === 'Interviewer'
                        var resultString = ''
                        if (isInterviewerRole) {
                            resultString +=
                                '<span class="interviewer"><a href="' +
                                self.config.api.profile +
                                '/' +
                                row.responsibleId +
                                '">' +
                                data +
                                '</a>'
                        } else {
                            resultString += '<span class="supervisor">' + data
                        }
                        resultString += '</span>'
                        return resultString
                    },
                },
                {
                    data: 'quantity',
                    name: 'Quantity',
                    class: 'type-numeric pointer editable',
                    searchHighlight: false,
                    searchable: false,
                    title: this.$t('Assignments.Size'),
                    tooltip: this.$t('Assignments.Tooltip_Table_Size'),
                    if() {
                        return self.config.isHeadquarter
                    },
                },
                {
                    data: 'interviewsCount',
                    name: 'InterviewsCount',
                    class: 'type-numeric',
                    title: this.$t('Assignments.Count'),
                    tooltip: this.$t('Assignments.Tooltip_Table_Count'),
                    orderable: true,
                    searchable: false,
                    render(data, type, row) {
                        var result =
                            "<a href='" + self.config.api.interviews + '?assignmentId=' + row.id + "'>" + data + '</a>'
                        return result
                    },
                    defaultContent: '<span>' + this.$t('Assignments.Unlimited') + '</span>',
                    if() {
                        return self.config.isHeadquarter
                    },
                },
                {
                    data: 'interviewsCount',
                    name: 'InterviewsCount',
                    class: 'type-numeric',
                    title: this.$t('Assignments.InterviewsNeeded'),
                    tooltip: this.$t('Assignments.Tooltip_Table_InterviewsNeeded'),
                    orderable: false,
                    searchable: false,
                    render(data, type, row) {
                        if (row.quantity < 0) return row.quantity
                        return row.interviewsCount > row.quantity ? 0 : row.quantity - row.interviewsCount
                    },
                    defaultContent: '<span>' + this.$t('Assignments.Unlimited') + '</span>',
                    if() {
                        return !self.config.isHeadquarter
                    },
                },
                {
                    data: 'identifyingQuestions',
                    title: this.$t('Assignments.IdentifyingQuestions'),
                    tooltip: this.$t('Assignments.Tooltip_Table_IdentifyingQuestions'),
                    class: 'prefield-column first-identifying last-identifying sorting_disabled visible',
                    orderable: false,
                    searchable: false,
                    render(data) {
                        const questionsWithTitles = _.map(data, question => {
                            return question.title + ': ' + question.answer
                        })
                        return _.join(questionsWithTitles, ', ')
                    },
                    responsivePriority: 4,
                },
                {
                    data: 'updatedAtUtc',
                    name: 'UpdatedAtUtc',
                    title: this.$t('Assignments.UpdatedAt'),
                    tooltip: this.$t('Assignments.Tooltip_Table_UpdatedAt'),
                    searchable: false,
                    render(data) {
                        var date = moment.utc(data)
                        return date.local().format(global.input.settings.clientDateTimeFormat)
                    },
                },
                {
                    data: 'createdAtUtc',
                    name: 'CreatedAtUtc',
                    title: this.$t('Assignments.CreatedAt'),
                    tooltip: this.$t('Assignments.Tooltip_Table_CreatedAt'),
                    searchable: false,
                    render(data) {
                        var date = moment.utc(data)
                        return date.local().format(global.input.settings.clientDateTimeFormat)
                    },
                },
                {
                    data: 'isAudioRecordingEnabled',
                    name: 'AudioRecording',
                    class: 'pointer editable',
                    title: this.$t('Assignments.IsAudioRecordingEnabled'),
                    tooltip: this.$t('Assignments.Tooltip_Table_IsAudioRecordingEnabled'),
                    searchable: false,
                    render(data) {
                        return data ? self.$t('Common.Yes') : self.$t('Common.No')
                    },
                },
                {
                    data: 'email',
                    name: 'Email',
                    title: this.$t('Assignments.Email'),
                    tooltip: this.$t('Assignments.Tooltip_Table_Email'),
                    searchable: false,
                },
                {
                    data: 'password',
                    name: 'Password',
                    title: this.$t('Assignments.Password'),
                    tooltip: this.$t('Assignments.Tooltip_Table_Password'),
                    searchable: false,
                },
                {
                    data: 'receivedByTabletAtUtc',
                    name: 'ReceivedByTabletAtUtc',
                    title: this.$t('Assignments.ReceivedByTablet'),
                    searchable: false,
                    render(data) {
                        return data != null ? self.$t('Common.Yes') : self.$t('Common.No')
                    },
                },
                {
                    data: 'webMode',
                    name: 'WebMode',
                    title: this.$t('Assignments.WebMode'),
                    tooltip: this.$t('Assignments.Tooltip_Table_WebMode'),
                    searchable: false,
                    render(data) {
                        return data ? self.$t('Common.Yes') : self.$t('Common.No')
                    },
                },
            ]
        },

        showSelectors() {
            return !this.config.isObserver && !this.config.isObserving
        },

        tableOptions() {
            const columns = this.tableOptionsraw.filter(x => x.if == null || x.if())

            var defaultSortIndex = _.findIndex(columns, {name: 'UpdatedAtUtc'})
            if (this.showSelectors) defaultSortIndex += 1

            var tableOptions = {
                rowId: function(row) {
                    return `row_${row.id}`
                },
                deferLoading: 0,
                order: [[defaultSortIndex, 'desc']],
                columns,
                ajax: {url: this.config.api.assignments, type: 'GET'},
                select: {
                    style: 'multi',
                    selector: 'td>.checkbox-filter',
                    info: false,
                },
                sDom: 'fr<"table-with-scroll"t>ip',
                headerCallback: thead => {
                    for (let i = 0; i < columns.length; i++) {
                        $(thead)
                            .find('th')
                            .eq(i)
                            .attr('title', columns[i].tooltip)
                    }
                },
                searchHighlight: true,
            }

            return tableOptions
        },
    },

    methods: {
        addParamsToRequest(requestData) {
            requestData.responsibleId = (this.responsibleId || {}).key
            requestData.questionnaireId = (this.questionnaireId || {}).key
            requestData.questionnaireVersion = (this.questionnaireVersion || {}).key
            requestData.showArchive = (this.showArchive || {}).key
            requestData.dateStart = this.dateStart
            requestData.dateEnd = this.dateEnd
            requestData.userRole = this.userRole
            requestData.receivedByTablet = (this.receivedByTablet || {}).key
            requestData.teamId = this.teamId
            requestData.id = this.id
        },

        userSelected(newValue) {
            this.responsibleId = newValue
        },

        questionnaireSelected(newValue) {
            this.questionnaireId = newValue
            this.questionnaireVersionSelected(null)
        },

        questionnaireVersionSelected(newValue) {
            this.questionnaireVersion = newValue
        },

        newResponsibleSelected(newValue) {
            this.newResponsibleId = newValue
        },

        receivedByTabletSelected(newValue) {
            this.receivedByTablet = newValue
        },

        showArchiveSelected(newValue) {
            this.showArchive = newValue
        },

        startWatchers(props, watcher) {
            var iterator = prop => this.$watch(prop, watcher)

            props.forEach(iterator, this)
        },

        reloadTable() {
            this.isLoading = true
            this.selectedRows.splice(0, this.selectedRows.length)
            this.$refs.table.reload(self.reloadTable)

            this.addParamsToQueryString()
        },

        addParamsToQueryString() {
            var queryString = {showArchive: this.showArchive.key}

            if (this.questionnaireId != null) {
                queryString.QuestionnaireId = this.questionnaireId.value
            }
            if (this.questionnaireVersion != null) {
                queryString.questionnaireVersion = this.questionnaireVersion.key
            }

            if (this.responsibleId) queryString.responsible = this.responsibleId.value
            if (this.dateStart) queryString.dateStart = this.dateStart
            if (this.dateEnd) queryString.dateEnd = this.dateEnd
            if (this.userRole) queryString.userRole = this.userRole
            if (this.receivedByTablet != null) queryString.receivedByTablet = this.receivedByTablet.key
            if (this.teamId) queryString.teamId = this.teamId
            if (this.id) queryString.id = this.id

            this.$router.push({query: queryString})
        },

        async archiveSelected() {
            await this.$http({
                method: 'delete',
                url: this.config.api.assignments,
                data: this.selectedRows,
            })

            this.reloadTable()
        },

        async unarchiveSelected() {
            await this.$http.post(this.config.api.assignments + '/Unarchive', this.selectedRows)

            this.reloadTable()
        },

        upateAudioRecording() {
            this.$hq.Assignments.setAudioSettings(this.editedRowId, this.editedAudioRecordingEnabled).then(() => {
                this.$refs.editAudioEnabledModal.hide()                
                this.reloadTable()
            })
        },

        assignSelected() {
            this.$refs.assignModal.modal({
                keyboard: false,
            })
        },

        closeSelected() {
            this.$refs.closeModal.modal({
                keyboard: false,
            })
        },

        async close() {
            const self = this
            await Promise.all(
                _.map(self.selectedRows, row => {
                    const url = `${self.config.api.assignmentsApi}/${row}/close`
                    return self.$http.post(url).catch(error => {
                        if (error.isAxiosError && error.response.status === 409) {
                            const msg = this.$t('Assignments.AssignmentCloseWebMode', {
                                id: row,
                            })

                            toastr.warning(msg)
                        }
                    })
                })
            )
            this.$refs.closeModal.hide()
            this.reloadTable()
        },

        async assign() {
            await this.$http.post(this.config.api.assignments + '/Assign', {
                responsibleId: this.newResponsibleId.key,
                comments: this.reassignComment,
                ids: this.selectedRows,
            })

            this.$refs.assignModal.hide()
            this.newResponsibleId = null
            this.reassignComment = null
            this.reloadTable()
        },

        cellClicked(columnName, rowId, cellData) {
            const parsedRowId = rowId.replace('row_', '')
            if (columnName === 'Quantity' && this.config.isHeadquarter && !this.showArchive.key) {
                this.editedRowId = parsedRowId
                this.editedQuantity = cellData

                this.$hq.Assignments.quantitySettings(this.editedRowId).then(data => {
                    this.canEditQuantity = data.CanChangeQuantity
                    this.$refs.editQuantityModal.modal('show')
                })                
            }
            if (columnName === 'AudioRecording' && this.config.isHeadquarter && !this.showArchive.key) {
                this.editedRowId = parsedRowId
                this.editedAudioRecordingEnabled = null
                this.$hq.Assignments.audioSettings(this.editedRowId).then(data => {
                    this.editedAudioRecordingEnabled = data.Enabled
                    this.$refs.editAudioEnabledModal.modal('show')
                })                
            }
        },
        async updateQuantity() {
            const validationResult = await this.$validator.validateAll()

            if (validationResult == false) {
                return false
            }

            const patchQuantityUrl = this.config.api.assignments + '/' + this.editedRowId + '/SetQuantity'

            let targetQuantity = null

            if (this.editedQuantity == null || this.editedQuantity === '') {
                targetQuantity = 1
            } else if (this.editedQuantity == -1) {
                targetQuantity = null
            } else {
                targetQuantity = this.editedQuantity
            }

            const self = this
            this.$http
                .patch(patchQuantityUrl, {
                    quantity: targetQuantity,
                })
                .then(() => {
                    this.$refs.editQuantityModal.hide()
                    this.editedQuantity = this.editedRowId = null
                    this.reloadTable()
                })
                .catch(error => {
                    self.errors.clear()
                    self.errors.add({
                        field: 'editedQuantity',
                        msg: error.response.data.message,
                        id: error.toString(),
                    })
                })

            return false
        },

        async loadResponsibleIdByName(onDone) {
            if (this.$route.query.responsible != undefined) {
                const requestParams = _.assign(
                    {
                        query: this.$route.query.responsible,
                        pageSize: 1,
                        cache: false,
                    },
                    this.ajaxParams
                )

                const response = await this.$http.get(this.config.api.responsible, {params: requestParams})

                onDone(response.data.options.length > 0 ? response.data.options[0].key : undefined)
            } else onDone()
        },

        async loadQuestionnaireId(onDone) {
            let requestParams = null

            const questionnaireId = this.$route.query.questionnaireId
            const version = this.$route.query.questionnaireVersion

            if (questionnaireId != undefined && version != undefined) {
                requestParams = _.assign(
                    {questionnaireIdentity: questionnaireId + '$' + version, cache: false},
                    this.ajaxParams
                )
                const response = await this.$http.get(this.config.api.questionnaireById, {params: requestParams})

                if (response.data) {
                    onDone(response.data.id, response.data.title, response.data.version)
                }
            } else onDone()
        },

        resetSelection() {
            this.selectedRows.splice(0, this.selectedRows.length)
        },
        formatNumber(value) {
            if (value == null || value == undefined) return value
            var language =
                (navigator.languages && navigator.languages[0]) || navigator.language || navigator.userLanguage
            return value.toLocaleString(language)
        },
    },

    mounted() {
        var self = this

        $('main').removeClass('hold-transition')
        $('footer').addClass('footer-adaptive')

        if (this.$route.query.showArchive != undefined && this.$route.query.showArchive === 'true')
            this.showArchiveSelected(this.ddlShowArchive[1])
        else this.showArchiveSelected(this.ddlShowArchive[0])

        this.dateStart = this.$route.query.dateStart
        this.dateEnd = this.$route.query.dateEnd
        this.userRole = this.$route.query.userRole
        this.teamId = this.$route.query.teamId
        this.id = this.$route.query.id

        this.receivedByTabletSelected(this.ddlReceivedByTablet[0])

        self.loadQuestionnaireId((questionnaireId, questionnaireTitle, version) => {
            if (questionnaireId != undefined) {
                self.questionnaireId = {
                    key: questionnaireId,
                    value: questionnaireTitle,
                }
                self.questionnaireVersion = {
                    key: version,
                    value: version,
                }
            }

            self.loadResponsibleIdByName(responsibleId => {
                if (responsibleId != undefined)
                    self.responsibleId = {key: responsibleId, value: self.$route.query.responsible}

                self.reloadTable()
                self.startWatchers(
                    ['responsibleId', 'questionnaireId', 'showArchive', 'receivedByTablet', 'questionnaireVersion'],
                    self.reloadTable.bind(self)
                )
            })
        })
    },
}
</script>
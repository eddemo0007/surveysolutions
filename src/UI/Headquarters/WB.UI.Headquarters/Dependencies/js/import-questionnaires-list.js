﻿'use strict';

$(function () {
    var questionnaireListUrl = $('#questionnaireListUrl').attr('href');
    var questionnaireImportModeUrl = $('#questionnaireImportModeUrl').attr('href');

    var requestHeaders = {};
    requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;

    var onTableInitComplete = function () {
        $('#DataTables_Table_0_filter label').on('click', function (e) {
            if (e.target !== this)
                return;
            if ($(this).hasClass("active")) {
                $(this).removeClass("active");
            }
            else {
                $(this).addClass("active");
            }
            $(".column-questionnaire-title").toggleClass("padding-left-lide");
        });
    };

    var table = $('table.import-interview')
        .on('init.dt', onTableInitComplete)
        .DataTable({
            processing: true,
            language:
            {
			    "processing": "<div>Loading, please wait</div>"
			},
            serverSide: true,
            ajax: {
                url: questionnaireListUrl,
                type: "POST",
                headers: requestHeaders
            },
            columns: [
                {
                    data: "title",
                    name: "Title", // case-sensitive!
                    render: function(data, type, row) {
                        return "<a href=" + questionnaireImportModeUrl + "/" + row.id + ">" + data + "</a>";
                    }
                },
                {
                    data: "lastModified",
                    name: "LastEntryDate", // case-sensitive! should be DB name here from Designer questionnairelistviewitems? to sort column
                    "class": "changed-recently"
                },
                {
                    data: "createdBy",
                    name: "CreatedBy",  // case-sensitive! should be DB name here from Designer DB questionnairelistviewitems? to sort column
                    "class": "created-by"
                }
            ],
            rowId: 'id',
            pagingType: "full_numbers", 
            lengthChange: false, // do not show page size selector
            pageLength: 50 // page size
        });
});
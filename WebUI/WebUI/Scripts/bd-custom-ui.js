
$.validator.methods.date = function (value, element) {
        var dateParts = value.split('/');
        var dateStr = '';
        var day = dateParts[0];
        if (day.length == 1)
            day = "0" + day;
        var month = dateParts[1];
        if (month.length == 1)
            month = "0" + month;
    if (countryConfig == 'AU')
    {
        dateStr = dateParts[2] + '-' + month + '-' + day;
    }
    else if (countryConfig == 'US')
    {
        dateStr = dateParts[2] + '-' + day + '-' + month;
    }
    
    return this.optional(element) || !/Invalid|NaN/.test(new Date(dateStr));
 
};

var defaultRangeValidator = $.validator.methods.range;
$.validator.methods.range = function (value, element, param) {
    if (element.type === 'checkbox') {
        // if it's a checkbox return true if it is checked
        return element.checked;
    } else {
        // otherwise run the default validation function
        return defaultRangeValidator.call(this, value, element, param);
    }
}

//Added to fix file upload bug #108. May have implications breaking  Request.IsAjaxRequest() server side
window.addEventListener("submit", function (e) {
    var form = e.target;
    if (form.getAttribute("enctype") === "multipart/form-data") {
        if (form.dataset.ajax) {
            e.preventDefault();
            e.stopImmediatePropagation();
            var xhr = new XMLHttpRequest();
            xhr.open(form.method, form.action);
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200) {
                    if (form.dataset.ajaxUpdate) {
                        var updateTarget = document.querySelector(form.dataset.ajaxUpdate);
                        if (updateTarget) {
                            updateTarget.innerHTML = xhr.responseText;
                        }
                    }
                }
            };
            xhr.send(new FormData(form));
        }
    }
}, true);

function HandleError(ajaxContext) {
    var statusCode = ajaxContext.status;
    alert("Sorry, the request failed with status code " + statusCode + "\n Error: '" + ajaxContext.responseText + "'");
}

function DocumentReady() {
    $.validator.unobtrusive.parse(document); //Added so that client validation is hooked up (http://stackoverflow.com/questions/5127813/call-mvc-3-client-side-validation-manually-for-ajax-posts)

    //Hookup any popover links
    $("a.popoverLink").popover({
        trigger: 'manual',
    }).click(function () {
        $(this).popover('toggle');
        $("a.popoverLink").not(this).popover('hide');
    });

    $(document).click(function (e) {
        if ($('a.popoverLink').length > 0) {
            if (!$(e.target).parent().hasClass('popoverLink')) {
                $("a.popoverLink").popover('hide');
            }
        }
    });

    if ($('.kc_fab_wrapper').length) {
        WeekView_RenderSpeeddialMenu();
    }
    //Hookup any JQuery tooltips
    // $(document).tooltip();
    $('[data-toggle="tooltip"]').tooltip()

    //Depending on browser support for HTML5 controls, hook up JQuery controls
    //if (!Modernizr.inputtypes['datetime-local']) {
    var minHour = null;
    if ($('#MinDateHour').val() != null)
        minHour = parseInt($('#MinDateHour').val());

    $("input[class~='bd-time']").each(function () {
        $(this).timepicker(
            {
                'showDuration': true,
                'timeFormat': 'H:i'
            });

        if ($(this).is("#StartTime")) {
            $(this).addClass("startPair");
        } else if ($(this).is("#FinishTime")) {
            $(this).addClass("endPair");
        }
    });


    $("input[class~='bd-date']").each(function () {

        var minDate = getMinDateFromAttributes($(this));
        var maxDate = getMaxDateFromAttributes($(this));

        $(this).datepicker(
            {
                format: getLocaleDateString(),
                'autoclose': true
            });
        $(this).datepicker("refresh");

        if ($(this).is("#StartDay")) {
            $(this).addClass("startPair");
        } else if ($(this).is("#FinishDay")) {
            $(this).addClass("endPair");
        }

    });

    $('#timeSection').datepair();

    $('#calendar').fullCalendar({
        header: {
            left: 'prev, next, today',
            center: 'title',
            right: 'month,agendaWeek,agendaDay'
        },
        // height: 'auto',
        //contentHeight: 'auto',
        windowResize: function (view) { },
        defaultView: 'agendaWeek',
        firstDay: 1,
        firstHour: 6,
        allDaySlot: false,
        timeFormat: 'H:mm', // uppercase H for 24-hour clock
        columnFormat: 'ddd D/M',
        editable: true,
        ignoreTimezone: false,
        disableDragging: true,
        droppable: false,
        editable: false,
        displayEventEnd: true, //Display end time in month views
        dayClick: function (date, allDay, jsEvent, view) {
            //createEvent(date);
        },
        eventClick: function (calEvent, jsEvent, view) {
            if (calEvent.type == "work") {
                $('#Id').val(calEvent.id);
                $('#BusinessLocationId').val(calEvent.businessLocationId);
                $('#Start').val(calEvent.start);
                $('#Finish').val(calEvent.end);
                $('#Length').val(calEvent.length);
                $('#HoursToStart').val(calEvent.hoursToShift);
                $('#Location').val(calEvent.location);

                FullCalendar_OnShowCancelShiftModal();
                $('#modalShiftDetails').modal('show');
            }
            else if (calEvent.type == "recurring") {
                $('#Id').val(calEvent.id);
                $('#BusinessLocationId').val(calEvent.businessLocationId);
                $('#DateTicks').val(calEvent.dateTicks);
                $('#Start').val(calEvent.start);
                $('#Finish').val(calEvent.end);
                $('#Length').val(calEvent.length);
                $('#HoursToStart').val(calEvent.hoursToShift);
                $('#Location').val(calEvent.location);
                $('#Role').val(calEvent.role);
                $('#BusinessName').val(calEvent.businessName);
                $('#BusinessLocationName').val(calEvent.businessLocationName);

                FullCalendar_OnShowCancelRecurringShiftModal();
                $('#modalRecurringShiftDetails').modal('show');
            }
        },
        events: {
            url: '/EmployeeRoster/GetSchedules',
            type: 'POST',
            error: function (msg) {
                alert('There was an error while fetching events!');
            },
            color: 'yellow',   // a non-ajax option
            textColor: 'black' // a non-ajax option
        }
    });

}

function RosterBroadcast_checkSelected() {
    var cbs = document.getElementsByTagName('input');
    var isSelected = false;
    for (var i = 0; i < cbs.length; i++) {
        if (cbs[i].checked == true) {
            isSelected = true;
        }
    }
    if (isSelected)
        return true;
    else {
        alert("You must select at least one shift to broadcast!");
        return false;
    }
}

function RosterBroadcast_checkAll(checked) {
    var cbs = document.getElementsByTagName('input');
    if (checked) {
        $("#btnSelectAll").hide();
        $("#btnDeselectAll").show();
    }
    else {
        $("#btnSelectAll").show();
        $("#btnDeselectAll").hide();
    }
    for (var i = 0; i < cbs.length; i++) {
        if (cbs[i].type == 'checkbox') {
            cbs[i].checked = checked;
        }
    }
}

function RosterBroadcast_busIntLocDdChanged(businessId, businessLocationId, monthDate, yearDate, location) {

    $.ajax({
        type: "GET",
        url: "/Roster/RosterBroadcast",
        data: { businessId: businessId, businessLocationId: businessLocationId, monthDate: monthDate, yearDate: yearDate, location: location },
        dataType: "html",
        success: function (data) {
            $('#body').html(data);
        },
        error: function (e) {
            alert('Error occurred: ' + e);
        }
    });
}

function RosterMonthView_busIntLocDdChanged(rosterid, location) {

    $.ajax({
        type: "GET",
        url: "/Roster/WeekView",
        data: { rosterId: rosterid, location: location },
        dataType: "html",
        success: function (data) {
            $('#body').html(data);
        },
        error: function (e) {
            alert('Error occurred: ' + e);
        }
    });
}


function RosterBroadcastShiftCheck_onChangeDDL(shiftId) {
    var selEmployeeId = $("#assignedEmployee_" + shiftId + " :selected").val();
    if (selEmployeeId != '')
        $("#assignShift_" + shiftId).show();
    else
        $("#assignShift_" + shiftId).hide();

}

function RosterBroadcastShiftCheck_assignShift(shiftId) {
    var selEmployeeId = $("#assignedEmployee_" + shiftId + " :selected").val();
    if (selEmployeeId) {

        $.ajax({
            url: "/Roster/AssignShift",
            type: 'POST',
            headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            contentType: 'application/json',
            data: JSON.stringify({ shiftId: shiftId, empId: selEmployeeId }),
            beforeSend: function (xhr) {
                //display spinner
                //var reqId = JSON.parse(this.data)['reqId'];
                $('#requestBtns_' + shiftId).hide();
                $('#requestWait_' + shiftId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
            },
            success: function (msg) {
                //On success remove spinner
                $("#assignedEmployee_" + shiftId)[0].disabled = true;
                $('#requestWait_' + shiftId).html('Shift Assigned!');
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert("Error: " + jqXHR.responseText);
                var reqId = JSON.parse(this.data)['reqId'];
                $('#requestWait_' + shiftId).html('');
                $('#requestBtns_' + shiftId).show();
            },
            statusCode: {
                404: function () {
                    alert("Error sending email: 404 page not found");
                }
            }
        })
    }
    else
        alert("You must select an employee");
}

function getMinDateFromAttributes(obj) {
    var minDate = null;

    if (obj.attr("data-MinDateYear") != null && obj.attr("data-MinDateMonth") != null && obj.attr("data-MinDateDay") != null) {
        var year = parseInt(obj.attr("data-MinDateYear"));
        var month = parseInt(obj.attr("data-MinDateMonth"));
        var day = parseInt(obj.attr("data-MinDateDay"));
        //For some reason must subtract 1 from month to work properly.
        minDate = new Date(year, month - 1, day);
    }
    return minDate;
}

function getMaxDateFromAttributes(obj) {
    var maxDate = null;

    if (obj.attr("data-MaxDateYear") != null && obj.attr("data-MaxDateMonth") != null && obj.attr("data-MaxDateDay") != null) {
        var year = parseInt(obj.attr("data-MaxDateYear"));
        var month = parseInt(obj.attr("data-MaxDateMonth"));
        var day = parseInt(obj.attr("data-MaxDateDay"));
        //For some reason must subtract 1 from month to work properly.
        maxDate = new Date(year, month - 1, day);
    }
    return maxDate;
}

function getEmployeeRoles(employeeId) {
    var retObj = null;
    $.ajax({
        url: "/Employee/GetEmployeeRoles/" + employeeId,
        type: 'GET',
        async: false,
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        success: function (msg) {
            retObj = JSON && JSON.parse(msg) || $.parseJSON(msg);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
    return retObj;
}

function getEmployeesInRole(roleId) {
    var retObj = null;
    $.ajax({
        url: "/Employee/GetEmployeesInRole/" + roleId,
        type: 'GET',
        async: false,
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        success: function (msg) {
            retObj = JSON && JSON.parse(msg) || $.parseJSON(msg);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
    return retObj;
}

function getUnavailableEmployees(businessLocationId, startTime, finishTime, shiftId) {
    var retObj = null;
    $.ajax({
        url: "/Roster/GetUnavailableEmployees?businessLocationId=" + businessLocationId + "&startTime=" + startTime + "&finishTime=" + finishTime + "&shiftId=" + shiftId,
        type: 'GET',
        async: false,
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        success: function (msg) {
            retObj = JSON && JSON.parse(msg) || $.parseJSON(msg);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
    return retObj;
}

function getNextRosterDate(busLocId) {
    var retObj = null;
    $.ajax({
        url: "/Roster/GetNextRosterDate/" + busLocId,
        type: 'GET',
        async: false,
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        success: function (msg) {
            retObj = JSON && JSON.parse(msg) || $.parseJSON(msg);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
    return retObj;
}

/*
    Modified by: Luis Arturo Salgado Orta
        1) Name of third parameter was changed to 'nextDayBool'
        2) Usage of minDate removed. Using current selected dates.
        3) if next day is true, gets next day from finish time selected date
*/
function ShiftCreate_ShiftBlockClick(st, ft, nextDayBool, roleId, minDate, nextDays) {


    $('#StartTime').val(st);
    $('#FinishTime').val(ft);
    var startDT = $('#StartDay').datepicker('getDate');

    if (nextDayBool == 'True') {
        var endDT = $('#StartDay').datepicker('getDate');
        endDT.setDate(endDT.getDate() + 1);
        $('#FinishDay').datepicker('setDate', endDT)
    }
    else {
        $('#FinishDay').datepicker('setDate', startDT)
    }

    // var startTime = st.split(":");
    // var finishTime = ft.split(":");

    //Depending on browser support for HTML5 controls, hook up JQuery controls
    // if (!Modernizr.inputtypes['datetime-local']) {
    //  var startDT = $('#StartTime').datepicker('getDate');
    // var endDT = $('#StartTime').datepicker('getDate');

    // startDT.setHours(parseInt(startTime[0]), parseInt(startTime[1]), 0, 0);
    // $('#StartTime').datepicker('setDate', startDT);

    //if (nextDayBool == 'True') {
    //    endDT.setDate(endDT.getDate() + 1);
    //}

    // endDT.setHours(parseInt(finishTime[0]), parseInt(finishTime[1]), 0, 0);
    //   $('#FinishTime').datepicker('setDate', endDT);
    //}
    //else //HTML5 native controls being used.
    //{
    //    var startDT = new Date($('#StartTime').val());
    //    var endDT = new Date($('#StartTime').val());

    //    startDT.setHours(parseInt(startTime[0]), parseInt(startTime[1]), 0, 0);

    //    $('#StartTime').val(moment(startDT.toISOString()).utcOffset('+1000').format('YYYY-MM-DDTHH:mm:ss'));

    //    if (nextDayBool == 'True') {
    //        endDT.setDate(endDT.getDate() + 1);
    //    }

    //    endDT.setHours(parseInt(finishTime[0]), parseInt(finishTime[1]), 0, 0);

    //    //TODO: Need to fix for different timezones DateTimeNowAEST
    //   $('#FinishTime').val(moment(endDT.toISOString()).utcOffset('+1000').format('YYYY-MM-DDTHH:mm:ss'));
    //}

    $('#roleDd').val(roleId);
    $('#roleDd').change();

    $("#SaveAsShiftBlock").attr('checked', false);
    $("#SaveAsShiftBlock").attr('disabled', 'disabled');
};

function ShiftCreate_isAvailable() {
    var avail = true;

    var busLocId = $('#BusinessLocationId').val();
    var startDT = $('#StartDay').val() + ' ' + $('#StartTime').val();
    var finishDT = $('#FinishDay').val() + ' ' + $('#FinishTime').val();
    var selEmpID = $('#employeeDd').val();
    var shiftId = $('#Id').val();

    if (selEmpID != '') {
        var unavailEmps = getUnavailableEmployees(busLocId, startDT, finishDT, shiftId);

        unavailEmps.forEach(function (emp) {
            if (emp.Id == selEmpID) {
                avail = false;
            }
        });
    }

    return avail;
};

function ShiftCreate_Submit() {

    var startDT = $('#StartTime').val();
    var finishDT = $('#FinishTime').val();
    if (startDT == '' || finishDT == '') {
        alert("You must first enter a valid start and finish time");
        return false;
    }

    var available = ShiftCreate_isAvailable();
    if (available)
        return true;
    else {
        alert("The selected employee is NOT available for this shift");
        return false;
    }
}

function ShiftCreate_CheckUnavailability() {
    var startDT = $('#StartTime').val();
    var finishDT = $('#FinishTime').val();
    if (startDT == '' || finishDT == '')
        alert("You must first enter a valid start and finish time");
    else {
        var selEmpID = $('#employeeDd').val();
        if (selEmpID == '')
            alert("You must first select an employee");
        else {
            var available = ShiftCreate_isAvailable();
            if (available)
                alert("Selected employee is available for this shift");
            else
                alert("The selected employee is NOT available for this shift");

        }
    }

}

function ShiftCreate_RoleDDChanged() {
    var selRolID = $('#roleDd').val();
    if ($('#employeeDd').val() == '' && selRolID != '') {
        //Reset selected value of list on any employee change
        $("#employeeDd option[value='']").attr("selected", "selected");
        var emps = getEmployeesInRole(selRolID);
        $("#employeeDd option[value!='']").attr('disabled', 'disabled');//.css('display','none');
        //Enable only options which are employees with the selected role
        emps.forEach(function (emp) {
            $("#employeeDd option[value='" + emp.Id + "']").removeAttr('disabled')
        });
    }
    else //Reset selected employee
    {
        $("#employeeDd option[value!='']").removeAttr('disabled');

    }

}

function ShiftCreate_EmployeeDDChanged() {
    var selEmpID = $('#employeeDd').val();
    //Only do anything if role DD hasn't already been selected
    if ($('#roleDd').val() == '' && selEmpID != '') {
        //Reset selected value of list on any employee change
        $("#roleDd option[value='']").attr("selected", "selected");
        var roles = getEmployeeRoles(selEmpID);
        $("#roleDd option[value!='']").attr('disabled', 'disabled');//.css('display','none');
        //Enable only options which are employees with the selected role
        roles.forEach(function (role) {
            $("#roleDd option[value='" + role.Id + "']").removeAttr('disabled');
        });
    }
    else //Reset selected employee
    {
        $("#roleDd option[value!='']").removeAttr('disabled');

    }
}

function RosterCreate_BusLocDDChanged() {
    var selBusLocID = $('#busLocDd').val();
    if (selBusLocID) {
        var nextDate = getNextRosterDate(selBusLocID);
        $('#WeekStartDate').val(nextDate);
        $('#Week_Starting').html(nextDate);
    }
}


//function FullCalendar_padDigits(n, totalDigits) {
//    n = n.toString(); var pd = '';
//    if (totalDigits > n.length) {
//        for (var i = 0; i < (totalDigits - n.length) ; i++) {
//            pd += '0';
//        }
//    } return pd + n.toString();
//}

//var startTime = null;
//var finishTime = null;
//var hoursTillShiftStart = null;

function FullCalendar_CancelShift() {
    //If preferences permit then allow cancellation
    //Get the business preferences
    var busLocId = $('#BusinessLocationId').val();
    var busPrefs = FullCalendar_getBusinessPreferences(busLocId);

    if (busPrefs != null
        && busPrefs.CancelShiftAllowed == true
        && ((busPrefs.CancelShiftTimeframe == 0) || (busPrefs.CancelShiftTimeframe == null)
             || (busPrefs.CancelShiftTimeframe < hoursTillShiftStart))) //Check if shift is outside allowable range to cancel
    {
        $("#modalShiftDetails").modal("hide");
        $("#modalShiftCancelAllowed").modal("show");
    }
    else {
        $("#modalShiftDetails").modal("hide");
        $('#divCancelShiftTimeframe').text(busPrefs.CancelShiftTimeframe);
        FullCalendar_getBusinessMgrSummary(busLocId);
        $("#modalShiftCancelNotAllowed").modal("show");
    }
}

function FullCalendar_OnShowCancelShiftModal() {
    var startTime = $('#Start').val();
    var finishTime = $('#Finish').val();
    var sDate = new Date(startTime);
    hoursTillShiftStart = $('#HoursToStart').val();
    $('#dlgSDShiftTime').text(startTime.substring(0, 21) + ' - ' + finishTime.substring(0, 21));
    $('#dlgSDShiftLength').text($('#Length').val());
    $('#dlgSDShiftLocation').text($('#Location').val());
    FullCalendar_getWorkingWithDetails($('#Id').val());
}

function FullCalendar_OnShowCancelRecurringShiftModal() {
    var startTime = $('#Start').val();
    var finishTime = $('#Finish').val();
    var sDate = new Date(startTime);
    $('#dlgRSDShiftTime').text(startTime + ' - ' + finishTime);
    $('#dlgRSDShiftLength').text($('#Length').val());
    $('#dlgRSDShiftLocation').text($('#Location').val());
    $('#dlgRSDShiftRole').text($('#Role').val());
    $('#dlgRSDShiftBusinessName').text($('#BusinessName').val());
    $('#dlgRSDShiftBusinessLocationName').text($('#BusinessLocationName').val());
}

function FullCalendar_CancelRecurringShift() {
    $("#modalRecurringShiftDetails").modal("hide");
    $("#modalRecurringShiftCancelAllowed").modal("show");
    // $('#dialogRecurringShiftCancelAllowed').dialog('open').data('Id', $(this).data('Id')).data('DateTicks', $(this).data('DateTicks'));
}

function FullCalendar_SendCancelRecurringShiftRequest() {
    if ($('#txtRCancelReason').val() == "") {
        alert("Error: reason must be entered");
    }
    else {
        //step1: Submit request to server
        $.ajax({
            url: "/EmployeeRoster/CancelRecurringShiftRequest",
            type: 'POST',
            headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            contentType: 'application/json',
            data: JSON.stringify({ shiftId: $('#Id').val(), reason: $('#txtRCancelReason').val(), shiftDate: $('#DateTicks').val() }),
            success: function (msg) {
                //step2: On Success reload the parent page
                alert("Success: Recurring shift cancellation requested");
                // location.reload(true);
                $('#txtRCancelReason').val('');
                $("#modalRecurringShiftCancelAllowed").modal("hide");
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert("Error: " + jqXHR.responseText);
            },
            statusCode: {
                404: function () {
                    alert("Error: 404 page not found");
                }
            }
        })

    }
}

function FullCalendar_SendCancelShiftRequest() {
    if ($('#txtCancelReason').val() == "") {
        alert("Error: reason must be entered");
    }
    else {
        //step1: Submit request to server
        $.ajax({
            url: "/EmployeeRoster/CancelShiftRequest",
            type: 'POST',
            headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            contentType: 'application/json',
            data: JSON.stringify({ shiftId: $('#Id').val(), reason: $('#txtCancelReason').val() }),
            success: function (msg) {
                //step2: On Success reload the parent page
                alert("Success: Shift cancellation requested");
                // location.reload(true);
                $('#txtCancelReason').val('');
                $("#modalShiftCancelAllowed").modal("hide");
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert("Error: " + jqXHR.responseText);
                $("#modalShiftCancelAllowed").modal("hide");
                $('#txtCancelReason').val('');
            },
            statusCode: {
                404: function () {
                    alert("Error: 404 page not found");
                }
            }
        })

    }
}

function FullCalendar_getWorkingWithDetails(shiftId) {
    $.ajax({
        url: "/EmployeeRoster/GetShiftWorkingWithDetails/" + shiftId,
        type: 'GET',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        success: function (msg) {
            $('#dlgSDWorkingWith').text('');
            var obj = JSON && JSON.parse(msg) || $.parseJSON(msg);
            if (obj.length > 0) {
                obj.forEach(function (entry) {
                    $('#dlgSDWorkingWith').append(entry);
                    $('#dlgSDWorkingWith').append('<br/>');
                });
            }
            else {
                $('#dlgSDWorkingWith').append('No one');
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
}
function FullCalendar_getBusinessMgrSummary(businessLocationId) {
    $.ajax({
        url: "/EmployeeRoster/GetBusinessLocationManagerDetails/" + businessLocationId,
        type: 'GET',
        async: false,
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        success: function (msg) {
            var obj = JSON && JSON.parse(msg) || $.parseJSON(msg);
            obj.forEach(function (entry) {
                $('#dvMgrDetails').append(entry.FullName);
                $('#dvMgrDetails').append(" <a href='tel:" + entry.MobilePhone + "'>" + entry.MobilePhone + "</a>");
                $('#dvMgrDetails').append('<br/>');
            });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
}

function FullCalendar_getBusinessPreferences(businessLocationId) {
    var busPrefs = null;
    $.ajax({
        url: "/EmployeeRoster/GetBusinessLocationPreferences/" + businessLocationId,
        type: 'GET',
        async: false,
        cache: false,
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        success: function (msg) {
            busPrefs = JSON.parse(msg);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
        },
    })
    return busPrefs;
}

function Schedule_DocumentReady() {
    Schedule_checkRecurrenceType();
    DocumentReady();
}

function Schedule_checkRecurrenceType() {
    var value = $("#Frequency").val();
    if (value > 0) {
        $("#FrequencyChoice").val(value);
        $("#rb-recurrence-event").click();
        $("#schedule-recurrence-choice").click();
    }

    if (jQuery("#rb-one-time-event").is(":checked")) {
        Schedule_hideRecurrenceControls();
    }

    // hide days and monthly options if daily frequency
    var value = $("#FrequencyChoice").val();
    if (value == 1) { // daily
        $("#day-selection").hide("fast");
        $("#monthly-interval-selection").hide("fast");
    }

    if (value == 2) { // weekly
        $("#day-selection").show("fast");
        $("#monthly-interval-selection").hide("fast");
        return;
    }

    if (value == 3) { // biweekly
        $("#day-selection").show("fast");
        $("#monthly-interval-selection").hide("fast");
        return;
    }

    if (value == 4) { // monthly
        $("#day-selection").show("fast");
        $("#monthly-interval-selection").show("fast");
    }
}

function Schedule_hideRecurrenceControls() {

    // hide the repeat weeks
    $("#schedule-recurrence-choice").hide("fast");

    // set the frequency value back to empty
    $("#FrequencyChoice").val("");

    // hide the end date
    $("#schedule-end-date").hide("fast");

    // hide the days of the week
    $("#day-selection").hide("fast");

    // hide the monthly interval options
    $("#monthly-interval-selection").hide("fast");
}

function Schedule_rb_one_time_eventClick() {
    Schedule_hideRecurrenceControls();
};

function Schedule_rb_recurrence_eventClick() {

    // show the repeat weeks
    $("#schedule-recurrence-choice").show("fast");

    // show the end date
    $("#schedule-end-date").show("fast");

};
function Schedule_EditLoad() {
    Schedule_DocumentReady();

    if ($("#StartTime").val() == '00:00' && $("#EndTime").val() == '23:59') {
        $("#cbAllDay").prop('checked', true);
        $("#schedule-start-time").hide("fast");
        $("#schedule-end-time").hide("fast");
    }

};
function Schedule_cbAllDay_Click() {
    var cb = $("#cbAllDay");
    if (cb.is(":checked")) {
        $("#schedule-start-time").hide("fast");
        $("#schedule-end-time").hide("fast");
        $("#StartTime").val('00:00:00');
        $("#EndTime").val('23:59:00');
    }
    else {
        $("#schedule-start-time").show("fast");
        $("#schedule-end-time").show("fast");
        $("#StartTime").val('00:00:00');
        $("#EndTime").val('00:00:00');
    }

}

function Schedule_recurrence_choiceClick() {

    var value = $("#FrequencyChoice").val();

    if (value == 1) { /* daily */
        $("#day-selection").hide("fast");
        $("#monthly-interval-selection").hide("fast");
        return;
    }

    if (value == 2) { /* weekly */
        $("#day-selection").show("fast");
        $("#monthly-interval-selection").hide("fast");
        return;
    }

    if (value == 3) { /* biweekly */
        $("#day-selection").show("fast");
        $("#monthly-interval-selection").hide("fast");
        return;
    }

    if (value == 4) { /* monthly */
        $("#day-selection").show("fast");
        $("#monthly-interval-selection").show("fast");
    }
};

function Employee_ApproveRequest() {
    $.ajax({
        url: "/Employer/ApproveEmployeeRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ reqId: $('#reqId').val() }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
            updateEmployeeRequestCount();
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $("#modalApprove").modal("hide");
            $('#requestWait_' + reqId).html('Request Approved!');
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
};

function Employee_RejectRequest() {
    $.ajax({
        url: "/Employer/RejectEmployeeRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ reqId: $('#reqId').val() }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $("#modalReject").modal("hide");
            $('#requestWait_' + reqId).html('Request Rejected!');
            updateEmployeeRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
};

function EmployerSearch_ReferEmployer() {
    var bValid = true;
    allFields.removeClass("ui-state-error");
    bValid = bValid && checkLength(name, "username", 3, 16);
    bValid = bValid && checkLength(email, "email", 6, 80);
    // From jquery.validate.js (by joern), contributed by Scott Gonzalez: http://projects.scottsplayground.com/email_address_validation/
    bValid = bValid && checkRegexp(email, /^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/i, "eg. ui@jquery.com");
    if (bValid) {
        $.ajax({
            url: "/Employer/Refer",
            type: 'POST',
            headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            contentType: 'application/json',
            data: JSON.stringify({ refName: name.val(), refEmail: email.val() }),
            success: function (msg) {
                $("#modalRefer").modal("hide");
                $("#divReferSuccess").show();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $("#modalRefer").modal("hide");
                $("#divReferFail").show();
            },
            statusCode: {
                404: function () {
                    alert("Error sending email: 404 page not found");
                }
            }
        });
    }
};

function EmployerSearch_RequestEmployer() {
    $.ajax({
        url: "/Employer/RequestEmployer",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ busId: $('#empId').val() }),
        beforeSend: function (xhr) {
            //display spinner
            var busId = JSON.parse(this.data)['busId'];
            $('#request_' + busId).hide();
            $('#requestWait_' + busId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (data, textStatus, jqXHR) {
            //On success remove spinner
            var busId = JSON.parse(this.data)['busId'];
            $('#requestWait_' + busId).html('Request sent!');
            $("#modalRequest").modal("hide");
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var busId = JSON.parse(this.data)['busId'];
            $('#requestWait_' + busId).html('');
            $('#request_' + busId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
};

function RegisterBusiness_ClickAdd(e) {

    var itemIndex = $("#internalLocations input.iHidden").length;
    e.preventDefault();
    //var newItem = $("<div class='form-group'<div class='col-md-offset-2 col-md-10'><input id='InternalLocations_" + itemIndex + "__Id' type='hidden' value='' class='iHidden'  name='InternalLocations[" + itemIndex + "].Id' /><input type='text' id='InternalLocations" + itemIndex + "__Name' name='InternalLocations[" + itemIndex + "].Name'/></div></div>");
    var newItem = $("<div class='form-group'><label class='col-md-2 control-label' for='BusinessLocation_InternalLocations_" + itemIndex + "__Name'>Name</label><div class='col-md-10'><input name='BusinessLocation.InternalLocations[" + itemIndex + "].Id' class='iHidden' id='BusinessLocation_InternalLocations_" + itemIndex + "__Id' type='hidden' value='00000000-0000-0000-0000-000000000000' data-val-required='The Id field is required.' data-val='true'><input name='BusinessLocation.InternalLocations[" + itemIndex + "].Name' id='BusinessLocation_InternalLocations_" + itemIndex + "__Name' class='form-control' type='text' value='' data-val-required='The Name field is required.' data-val='true' data-val-maxlength-max='40' data-val-maxlength='The field Name must be a string or array type with a maximum length of '40'.'></div></div>");
    $("#internalLocations").append(newItem);
};
function EditBusiness_ClickAdd(e) {

    var itemIndex = $("#internalLocations input.iHidden").length;
    e.preventDefault();
    var newItem = $("<div class='form-group'><label class='col-md-2 control-label' for='InternalLocations_" + itemIndex + "__Name'>Name</label><div class='col-md-10'><input name='InternalLocations[" + itemIndex + "].Id' class='iHidden' id='InternalLocations_" + itemIndex + "__Id' type='hidden' value='00000000-0000-0000-0000-000000000000' data-val-required='The Id field is required.' data-val='true'><input name='InternalLocations[" + itemIndex + "].Name' id='InternalLocations_" + itemIndex + "__Name' class='form-control' type='text' value='' data-val-required='The Name field is required.' data-val='true' data-val-maxlength-max='40' data-val-maxlength='The field Name must be a string or array type with a maximum length of '40'.'></div></div>");
    $("#internalLocations").append(newItem);
};
function CreateBusinessDocumentReady() {
    if ($('#addrAuto').length > 0) {
        $.getScript("../Scripts/bd-googlemaps.js", function (jd) {
            // Call custom function defined in script
            initialize();
        });
    }

    DocumentReady();

    jQuery.validator.unobtrusive.adapters.add("dropdown", function (options) {
        //  
        if (options.element.tagName.toUpperCase() == "SELECT" && options.element.type.toUpperCase() == "SELECT-ONE") {
            options.rules["required"] = true;
            if (options.message) {
                options.messages["required"] = options.message;
            }
        }
    });


    if ($('#rbHasMultiBusLocationsYes').length > 0 && $('#rbHasMultiBusLocationsYes')[0].checked) {
        $('#btnAddAnotherLocation').show('fast');
        $('#submit').hide('fast');
        $('#fgBusLocationName').show('fast');
    }

    if ($('#rbHasMultiInternalLocationsYes').length > 0 && $('#rbHasMultiInternalLocationsYes')[0].checked)
        $('#sectionInternalLocations').show('fast');
}

function TypeIndustryDDL_OnChange() {
    //  
    var industry = $("#TypeIndustry :selected").val();
    if (industry != "") {
        $.ajax({
            type: "GET",
            contentType: "application/json; charset=utf-8",
            url: '/Business/GetBusinessTypes',
            data: { "industryName": industry },
            dataType: "json",
            beforeSend: function () {
                //alert(id);
            },
            success: function (data) {
                var items = "<option value=''>-- SELECT --</option>";
                $.each(data, function (i, businessType) {
                    items += "<option value='" + businessType.Id + "'>" + businessType.Detail + "</option>";
                });
                $('#TypeId').html(items);
            },
            error: function (result) {
                alert('Service call failed: ' + result.status + ' Type :' + result.statusText);
            }
        });
    }
    else {
        var items = "<option value=''>-- SELECT --</option>";
        $('#TypeId').html(items);
    }
};

function Roles_DisableRole() {
    $.ajax({
        url: "/Business/DisableRole",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ roleId: $('#roleId').val(), businessId: $('#businessId').val() }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['roleId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['roleId'];
            $('#edit_' + reqId).hide();
            $("#modalDisable").modal("hide");
            $('#requestWait_' + reqId).html('Role deleted successfully!');
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    });
}

function IntLocations_DisableInternalLocation() {
    $.ajax({
        url: "/Business/DisableInternalLocation",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ locId: $('#locId').val(), businessLocationId: $('#businessLocationId').val(), businessId: $('#businessId').val() }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['locId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['locId'];
            $('#edit_' + reqId).hide();
            $("#modalDisable").modal("hide");
            $('#requestWait_' + reqId).html('Location disabled successfully!');
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })

}

function IntLocations_EnableInternalLocation() {
    $.ajax({
        url: "/Business/EnableInternalLocation",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ locId: $('#locId').val(), businessLocationId: $('#businessLocationId').val(), businessId: $('#businessId').val() }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['locId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['locId'];
            $('#edit_' + reqId).show();
            $("#modalEnable").modal("hide");
            $('#requestWait_' + reqId).html('Location enabled successfully!');
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
}
function CreateEmployee_DocumentReady() {

    var modalAddRole = function () {
        $("#modalAddRole").modal("show");
    };
    $("#btnAddRole").click(modalAddRole);

    $('#modalAddRole').on('hidden.bs.modal', function () {
        $("#txtRoleName").val('');
    })

    $('#dialogContact').on('hidden.bs.modal', function () {
        if ($('#chkShow').prop('checked') == true)
            $.cookie('dialogContactDoNotShow', true);  //Set a cookie value
        else
            $.cookie('dialogContactDoNotShow', false);
    });

    //$('#busLocDd').change(function () {
    //    window.location = "/Employee/Create?businessId=" + $('#BusinessId').val() + "&businesslocationId=" + $('#busLocDd').val();

    //});

    if ($("#AddedAnother").val() == "True") {
        $("#dvSuccessAddAnother").show();
    }
    else {
        $("#dvSuccessAddAnother").hide();
    }

    DocumentReady();
}
function CreateEmployee_AddRole() {
    if ($('#txtRoleName').val() == "") {
        alert("Error: role cannot be empty");
        return;
    }
    $.ajax({
        url: "/Business/AddRole",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ roleName: $("#txtRoleName").val(), businessId: $("#BusinessId").val() }),
        success: function (data, textStatus, jqXHR) {
            $('#tblRoles tr:last').after("<tr><td><input type='checkbox' name='selectedRoles' value='" + data + "' checked='checked' />" + $("#txtRoleName").val() + "</td></tr>");
            $('#lblNoRoles').hide();
            $("#modalAddRole").modal("hide");
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);

        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
};

function CreateEmployee_DisplayPopup() {
    if ($('#createEmpForm').valid() //only display popup if any otehr validation is passed for the form
        && $('#MobilePhone').val().length == 0
        && $('#Email').val().length == 0
        && $.cookie('dialogContactDoNotShow') != 'true') {
        return true;
    }
    else
        return false;
};
function CreateRecurringShift_employeeDdChange() {
    //Reset selected value of list on any employee change
    $("#roleDd option[value='']").attr("selected", "selected");
    var selEmpID = $('#employeeDd').val();
    if (selEmpID) {
        var empRoles = getEmployeeRoles(selEmpID);
        $("#roleDd option").attr('disabled', 'disabled');
        if (empRoles.length == 1) {
            //Set corresponding role in droplist to be selected
            $("#roleDd option[value='" + empRoles[0].Id + "']").attr("selected", "selected");
        }
        //Enable only options which are roles for the selected employee
        empRoles.forEach(function (role) {
            $("#roleDd option[value='" + role.Id + "']").removeAttr('disabled')
        });
    }
    else {
        $("#roleDd option").removeAttr('disabled')
    }
}
function EditRecurringShift_DocumentReady() {

    //When opened for first time also need to disable unsupported roles
    var selEmpID = $('#employeeDd').val();
    if (selEmpID != "") {
        var empRoles = getEmployeeRoles(selEmpID);
        $("#roleDd option").attr('disabled', 'disabled');
        //Enable only options which are roles for the selected employee
        empRoles.forEach(function (role) {
            $("#roleDd option[value='" + role.Id + "']").removeAttr('disabled')
        });
    }

    DocumentReady();
}

function StaffRequest_ApproveRequest() {
    {
        $.ajax({
            url: "/Manager/ApproveRequest",
            type: 'POST',
            headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            contentType: 'application/json',
            data: JSON.stringify({ reqId: $('#empReqId').val() }),
            beforeSend: function (xhr) {
                //display spinner
                var reqId = JSON.parse(this.data)['reqId'];
                $('#requestBtns_' + reqId).hide();
                $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
            },
            success: function (msg) {
                //On success remove spinner
                var reqId = JSON.parse(this.data)['reqId'];
                $("#empModalApprove").modal("hide");
                $('#requestWait_' + reqId).html('Request Approved!');
                updateEmployerRequestCount();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert("Error: " + jqXHR.responseText);
                var reqId = JSON.parse(this.data)['reqId'];
                $('#requestWait_' + reqId).html('');
                $('#requestBtns_' + reqId).show();
            },
            statusCode: {
                404: function () {
                    alert("Error sending email: 404 page not found");
                }
            }
        })
    }
}

function StaffRequest_RejectRequest() {
    $.ajax({
        url: "/Manager/RejectRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ reqId: $('#empReqId').val() }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $("#empModalReject").modal("hide");
            $('#requestWait_' + reqId).html('Request Rejected!');
            updateEmployerRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
}

function ShiftRequest_ClearModalValues() {
    $('#shiftReqId').val('');
    $('#openShiftId').val('');
    $('#shiftIsRecurring').val('')
    $('#shiftIsOpenShift').val('');
    $('#txtAcceptReason').val('');
    $('#txtRejectReason').val('');
}

function ShiftRequest_ApproveRequest() {
    $.ajax({
        url: "/Manager/ApproveShiftChangeRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({
            reqId: $('#shiftReqId').val(), openShiftId: $('#openShiftId').val(), reason: $('#shiftTxtAcceptReason').val(), isRecurring: $('#shiftIsRecurring').val(), isOpenShift: $('#shiftIsOpenShift').val()
    }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            var openShiftId = JSON.parse(this.data)['openShiftId'];
            var isOpenShift = (JSON.parse(this.data)['isOpenShift'] === 'true'); //convert to boolean
            $("#shiftModalApprove").modal("hide");
            $('#requestWait_' + reqId).html('Request Approved!');
            //if it is an open shift hide all other buttons once one is approved
            if (isOpenShift == true) {
                $('div[id^="' + openShiftId + '"][class!="' + reqId + '"]').hide();
            }
            updateShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
    ShiftRequest_ClearModalValues();
}


function ShiftRequest_RejectRequest() {
    $.ajax({
        url: "/Manager/RejectShiftChangeRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ reqId: $('#shiftReqId').val(), reason: $('#shiftTxtRejectReason').val(), isRecurring: $('#shiftIsRecurring').val(), isOpenShift: $('#isOpenShift').val() }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('Request Rejected!');
            $("#shiftModalReject").modal("hide");
            updateShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
    ShiftRequest_ClearModalValues();
}

function updateEmployeeRequestCount() {
    return $.ajax({
        url: "/Request/GetEmployeeRequestCount",
        type: 'GET',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        async: true,
        success: function (requestCount) {
            if (requestCount != 0) {
                //Update the badge to reflect new count
                $('#employeeRequestBadge').html(requestCount);
                $('#settingsBadge').html(requestCount);
            }
            else {
                $('#employeeRequestBadge').html('');
                $('#settingsBadge').html('');
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
        }
    });
}


function updateTotalRequestCount() {
    return $.ajax({
        url: "/Request/GetTotalRequestCount",
        type: 'GET',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        async: true,
        success: function (requestCount) {
            if (requestCount != 0) {
                //Update the badge to reflect new count
                $('#totalRequestsBadge').html(requestCount);
            }
            else {
                $('#totalRequestsBadge').html('')
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
        }
    });
}

function updateEmployerRequestCount() {
    updateTotalRequestCount();
}

function updateShiftRequestCount() {
    updateTotalRequestCount();
}

function updateExternalShiftRequestCount() {
    updateTotalRequestCount();
}

function Notifications_GetAllRequest() {
    $.ajax({
        url: "/Manager/GetAllRequests",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        async: true,
        success: ShowAllNotifications,
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
        }
    });
}

Date.prototype.getFullMinutes = function () {
    if (this.getMinutes() < 10) {
        return '0' + this.getMinutes();
    }
    return this.getMinutes();
};
Date.prototype.getFullSeconds = function () {
    if (this.getSeconds() < 10) {
        return '0' + this.getSeconds();
    }
    return this.getSeconds();
};


function ShowAllNotifications(results) {

    if (results.length == 0) {
        $("#notifications").html('<p><em>You have no current notifications</em></p>');

    }
    else {

        $("#notifications").val('');
        results.sort(sortNotificationsFunction);
        for (var i = 0; i < results.length; i++) {
            var requestType = results[i].RequestType;

            switch (requestType) {
                case 0: //RequestTypeDTO.Employer = 0,
                    renderEmployerNotification(results[i]);
                    break;
                case 1: //RequestTypeDTO.ShiftCancel = 1
                    renderShiftCancelNotification(results[i]);
                    break;
                case 2: //RequestTypeDTO.TakeOpenShift = 2
                    renderTakeOpenShiftNotification(results[i]);
                    break;
                case 3: //RequestTypeDTO.RecurringShiftCancel = 3
                    renderRecurringShiftCancelNotification(results[i]);
                    break;
                case 4: //RequestTypeDTO.Employee = 4
                    renderEmployeeNotification(results[i]);
                    break;
                case 5: //RequestTypeDTO.TakeExternalShiftBroadCast
                    renderTakeExternalShiftNotification(results[i]);

            }

        }
        DocumentReady();
    }
}

function renderEmployeeNotification(request) {

    var dateValue = new Date(parseInt(request.CreatedDate.substr(6)));

    var htmlValue = '';
    htmlValue = '<li><div class="list-image"><span class="icon-message-text icon-md text-gray"></span></div>';
    htmlValue = htmlValue.concat('<div class="list-text"><span class="actions pull-right" ><div id="requestWait_' + request.Id + '"></div><div id="requestBtns_' + request.Id + '"> ')
    htmlValue = htmlValue.concat('<a href="#" class="btn btn-xs btn-block btn-primary" onclick="javascript: Dashboard_AproveEmployeeRequest(\'' + request.Id + '\'); return false;">Approve</a><a href="#" class="btn btn-xs btn-block btn-default" onclick="javascript: Dashboard_RejectEmployeeRequest(\'' + request.Id + '\'); return false;">Decline</a></div> </span>')
    htmlValue = htmlValue.concat(' <h5>Employee request</h5> ');
    htmlValue = htmlValue.concat('\'' + request.BusinessName + '\' requested to add you as an employee at location \'' + request.BusinessLocationName + '\'')
    htmlValue = htmlValue.concat('<small>' + dateValue.toDateString() + ' ' + dateValue.getHours() + ':' + dateValue.getFullMinutes() + ':' + dateValue.getFullSeconds() + '</small>');
    htmlValue = htmlValue.concat(' </div> </li>');

    $("#notifications").append(htmlValue);

}

function Dashboard_AproveEmployeeRequest(id) {
    $.ajax({
        url: "/Employer/ApproveEmployeeRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ reqId: id }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('Request Approved!');
            updateEmployeeRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
};

function Dashboard_RejectEmployeeRequest(id) {
    $.ajax({
        url: "/Employer/RejectEmployeeRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ reqId: id }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('Request Rejected!');
            updateEmployeeRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
};
function renderRecurringShiftCancelNotification(request) {

    var dateValue = new Date(parseInt(request.CreatedDate.substr(6)));
    var shiftStartTime = new Date(parseInt(request.StartDateTime.substr(6)));
    var shiftFinishTime = new Date(parseInt(request.FinishDateTime.substr(6)));

    var htmlValue = '';
    htmlValue = '<li><div class="list-image"><span class="icon-message-star icon-md text-secondary"></span></div>';
    htmlValue = htmlValue.concat('<div class="list-text"><span class="actions pull-right" ><div id="requestWait_' + request.Id + '"></div><div id="requestBtns_' + request.Id + '"> ')
    htmlValue = htmlValue.concat('<a href="#" class="btn btn-xs btn-block btn-primary" onclick="javascript: Dashboard_AproveShiftRequest(\'' + request.Id + '\',\'\', \'\', true, false); return false;">Approve</a><a href="#" class="btn btn-xs btn-block btn-default" onclick="javascript: Dashboard_RejectShiftRequest(\'' + request.Id + '\', \'\', true, false); return false;">Decline</a></div> </span>')
    htmlValue = htmlValue.concat(' <h5>Recurring shift cancel request</h5> ');
    htmlValue = htmlValue.concat(request.RequesterName + ' requested to cancel a recurring shift at ' + request.BusinessLocationName + ' from ' + shiftStartTime.toDateString() + ' ' + shiftStartTime.getHours() + ':' + shiftStartTime.getFullMinutes() + ' till ' + shiftFinishTime.getHours() + ':' + shiftFinishTime.getFullMinutes())
    htmlValue = htmlValue.concat('<small>' + dateValue.toDateString() + ' ' + dateValue.getHours() + ':' + dateValue.getFullMinutes() + ':' + dateValue.getFullSeconds() + '</small>');
    htmlValue = htmlValue.concat(' </div> </li>');

    $("#notifications").append(htmlValue);

}

function renderTakeOpenShiftNotification(request) {

    var dateValue = new Date(parseInt(request.CreatedDate.substr(6)));
    var shiftStartTime = new Date(parseInt(request.StartDateTime.substr(6)));
    var shiftFinishTime = new Date(parseInt(request.FinishDateTime.substr(6)));

    var htmlValue = '';
    htmlValue = '<li><div class="list-image"><span class="icon-message-text icon-md text-success"></span></div>';
    htmlValue = htmlValue.concat('<div class="list-text"><span class="actions pull-right" ><div id="requestWait_' + request.Id + '"></div><div id="requestBtns_' + request.Id + '"> ')
    htmlValue = htmlValue.concat('<a href="#" class="btn btn-xs btn-block btn-primary" onclick="javascript: Dashboard_AproveShiftRequest(\'' + request.Id + '\',\'' + request.ShiftId + '\', \'\', false, true); return false;">Approve</a><a href="#" class="btn btn-xs btn-block btn-default" onclick="javascript: Dashboard_RejectShiftRequest(\'' + request.Id + '\', \'\', false, true); return false;">Decline</a></div> </span>')
    htmlValue = htmlValue.concat(' <h5>Open shift request</h5> ');
    htmlValue = htmlValue.concat(request.RequesterName + ' requested to take open shift at ' + request.BusinessLocationName + ' from ' + shiftStartTime.toDateString() + ' ' + shiftStartTime.getHours() + ':' + shiftStartTime.getFullMinutes() + ' till ' + shiftFinishTime.getHours() + ':' + shiftFinishTime.getFullMinutes())
    htmlValue = htmlValue.concat('<small>' + dateValue.toDateString() + ' ' + dateValue.getHours() + ':' + dateValue.getFullMinutes() + ':' + dateValue.getFullSeconds() + '</small>');
    htmlValue = htmlValue.concat(' </div> </li>');

    $("#notifications").append(htmlValue);

}

function renderTakeExternalShiftNotification(request) {

    var dateValue = new Date(parseInt(request.CreatedDate.substr(6)));
    var shiftStartTime = new Date(parseInt(request.StartDateTime.substr(6)));
    var shiftFinishTime = new Date(parseInt(request.FinishDateTime.substr(6)));

    var htmlValue = '';
    htmlValue = '<li><div class="list-image"><span class="icon-message-text icon-md text-success"></span></div>';
    htmlValue = htmlValue.concat('<div class="list-text"><span class="actions pull-right" ><div id="requestWait_' + request.Id + '"></div><div id="requestBtns_' + request.Id + '"> ')
    htmlValue = htmlValue.concat('<a href="#" class="btn btn-xs btn-block btn-primary" onclick="javascript: Dashboard_AproveExternalShiftRequest(\'' + request.Id + '\',\'' + request.ExternalShiftBroadCastID + '\', \'\', false, true); return false;">Approve</a><a href="#" class="btn btn-xs btn-block btn-default" onclick="javascript: Dashboard_RejectExternalShiftRequest(\'' + request.Id + '\', \'\', false, true); return false;">Decline</a></div> </span>')
    htmlValue = htmlValue.concat(' <h5>External shift request</h5> ');
    htmlValue = htmlValue.concat(request.RequesterName + ' requested to take External shift ');
    htmlValue = htmlValue.concat('<small>' + dateValue.toDateString() + ' ' + dateValue.getHours() + ':' + dateValue.getFullMinutes() + ':' + dateValue.getFullSeconds() + '</small>');
    htmlValue = htmlValue.concat(' </div> </li>');

    $("#notifications").append(htmlValue);

}

function renderShiftCancelNotification(request) {

    var dateValue = new Date(parseInt(request.CreatedDate.substr(6)));
    var shiftStartTime = new Date(parseInt(request.StartDateTime.substr(6)));
    var shiftFinishTime = new Date(parseInt(request.FinishDateTime.substr(6)));

    var htmlValue = '';
    htmlValue = '<li><div class="list-image"><span class="icon-message icon-md text-danger"></span></div>';
    htmlValue = htmlValue.concat('<div class="list-text"><span class="actions pull-right" ><div id="requestWait_' + request.Id + '"></div><div id="requestBtns_' + request.Id + '"> ')
    htmlValue = htmlValue.concat('<a href="#" class="btn btn-xs btn-block btn-primary" onclick="javascript: Dashboard_AproveShiftRequest(\'' + request.Id + '\', \'\', \'\', false, false);  SendDataToModalInput(\'' + request.BusinessLocationId + '\', \'' + request.ShiftId + '\'); $(\'#modalReassign\').modal(\'show\'); return false;">Approve</a><a href="#" class="btn btn-xs btn-block btn-default" onclick="javascript: Dashboard_RejectShiftRequest(\'' + request.Id + '\', \'\', false, false); return false;">Decline</a></div> </span>')
    htmlValue = htmlValue.concat(' <h5>Shift cancel request</h5> ');
    htmlValue = htmlValue.concat(request.RequesterName + ' requested to cancel shift at ' + request.BusinessLocationName + ' from ' + shiftStartTime.toDateString() + ' ' + shiftStartTime.getHours() + ':' + shiftStartTime.getFullMinutes() + ' till ' + shiftFinishTime.getHours() + ':' + shiftFinishTime.getFullMinutes());
    htmlValue = htmlValue.concat(' <a style="cursor: pointer" data-toggle="popover" class="popoverLink" data-original-title="Cancellation reason" data-content="' + request.Reason + '"><i class="glyphicon glyphicon-question-sign"></i></a>');
    htmlValue = htmlValue.concat('<small>' + dateValue.toDateString() + ' ' + dateValue.getHours() + ':' + dateValue.getFullMinutes() + ':' + dateValue.getFullSeconds() + '</small>');
    htmlValue = htmlValue.concat(' </div> </li>');

    $("#notifications").append(htmlValue);

}
function renderEmployerNotification(request) {

    var dateValue = new Date(parseInt(request.CreatedDate.substr(6)));
    var htmlValue = '<li><div class="list-image"><span class="icon-message-star icon-md text-warning"></span></div>';
    htmlValue = htmlValue.concat('<div class="list-text"><span class="actions pull-right" ><div id="requestWait_' + request.Id + '"></div><div id="requestBtns_' + request.Id + '"> <a href="#" class="btn btn-xs btn-block btn-primary" onclick="javascript: Dashboard_AcceptEmployerRequest(\'' + request.Id + '\'); return false;">Approve</a><a href="#" class="btn btn-xs btn-block btn-default" onclick="javascript: Dashboard_RejectEmployerRequest(\'' + request.Id + '\'); return false;">Decline</a></div> </span>')
    htmlValue = htmlValue.concat(' <h5>Employer request</h5> ');
    htmlValue = htmlValue.concat(request.RequesterName + ' requested to be linked as an employee at ' + request.BusinessLocationName)
    htmlValue = htmlValue.concat('<small>' + dateValue.toDateString() + ' ' + dateValue.getHours() + ':' + dateValue.getMinutes() + ':' + dateValue.getFullSeconds() + '</small>');
    htmlValue = htmlValue.concat(' </div> </li>');

    $("#notifications").append(htmlValue);
}
function Dashboard_RejectShiftRequest(id, rejectReason, isRecurringShift, isOpen) {
    $.ajax({
        url: "/Manager/RejectShiftChangeRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ reqId: id, reason: rejectReason, isRecurring: isRecurringShift, isOpenShift: isOpen }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('Rejected!');
            updateShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
    ShiftRequest_ClearModalValues();
}

function ExternalShiftRequest_ClearModalValues() {
    $('#extShftReqId').val('');
    $('#externalShiftId').val('');
    $('#extShftIsRecurring').val('')
    $('#extShftIsOpenShift').val('');
    $('#txtAcceptReason').val('');
    $('#extShftTxtRejectReason').val('');
}


function ExterenalShiftRequest_ApproveRequest() {
    $.ajax({
        url: "/Manager/ApproveExternalShiftRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({
            reqId: $('#extShftReqId').val(), externalShiftId: $('#externalShiftId').val(), reason: $('#txtAcceptReason').val(), isRecurring: $('#extShftIsRecurring').val(), isOpenShift: $('#extShftIsOpenShift').val()
    }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            var externalShiftId = JSON.parse(this.data)['externalShiftId'];
            var isOpenShift = (JSON.parse(this.data)['isOpenShift'] === 'true'); //convert to boolean
            $("#extShftModalApprove").modal("hide");
            $('#requestWait_' + reqId).html('Request Approved!');
            //if it is an open shift hide all other buttons once one is approved
            if (isOpenShift == true) {
                $('div[id^="' + externalShiftId + '"][class!="' + reqId + '"]').hide();
            }
            updateExternalShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
    ExternalShiftRequest_ClearModalValues();
}

function Dashboard_RejectExternalShiftRequest(id, rejectReason, isRecurringShift, isOpen) {
    $.ajax({
        url: "/Manager/RejectExternalShiftRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ reqId: id, reason: rejectReason, isRecurring: isRecurringShift, isOpenShift: isOpen }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('Rejected!');
            updateShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
    ExternalShiftRequest_ClearModalValues();
}

function ExternalShiftRequest_RejectRequest() {
    $.ajax({
        url: "/Manager/RejectExternalShiftRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ reqId: $('#extShftReqId').val(), reason: $('#extShftTxtRejectReason').val(), isRecurring: $('#extShftIsRecurring').val(), isOpenShift: $('#extShftIsOpenShift').val() }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('Request Rejected!');
            $("#extShftModalReject").modal("hide");
            updateExternalShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
    ExternalShiftRequest_ClearModalValues();
}


function Dashboard_AproveExternalShiftRequest(id, externalShiftId, cancelReason, isRecurringShift, isOpen) {
    $.ajax({
        url: "/Manager/ApproveExternalShiftRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ reqId: id, externalShiftId: externalShiftId, reason: cancelReason, isRecurring: isRecurringShift, isOpenShift: isOpen }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            var externalShiftId = JSON.parse(this.data)['externalShiftId'];
            var isOpenShift = (JSON.parse(this.data)['isOpenShift'] === 'true'); //convert to boolean
            $('#requestWait_' + reqId).html('Approved!');
            updateExternalShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
}

function ProfileReview_AproveExternalShiftRequest() {
    $.ajax({
        url: "/Manager/ApproveExternalShiftRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ reqId: $('#reqId').val(), externalShiftId: $('#externalShiftId').val(), reason: $('#txtAcceptReason').val(), isRecurring: $('#isRecurring').val(), isOpenShift: $('#isOpenShift').val() }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#approve_' + reqId).prop('disabled', true);
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            var externalShiftId = JSON.parse(this.data)['externalShiftId'];
            var isOpenShift = (JSON.parse(this.data)['isOpenShift'] === 'true'); //convert to boolean
            $("#modalApprove").modal("hide");
            $('#requestWait_' + reqId).html('');
            $('#approve_' + reqId).prop('disabled', true);
            $('#approve_' + reqId).html('Request Approved!');
            //if it is an open shift hide all other buttons once one is approved
            if (isOpenShift == true) {
                $('div[id^="' + externalShiftId + '"][class!="' + reqId + '"]').hide();
            }
            updateExternalShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
    ExternalShiftRequest_ClearModalValues();
}

function Dashboard_AproveShiftRequest(id, openShiftId, cancelReason, isRecurringShift, isOpen) {
    $.ajax({
        url: "/Manager/ApproveShiftChangeRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ reqId: id, openShiftId: openShiftId, reason: cancelReason, isRecurring: isRecurringShift, isOpenShift: isOpen }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            var openShiftId = JSON.parse(this.data)['openShiftId'];
            var isOpenShift = (JSON.parse(this.data)['isOpenShift'] === 'true'); //convert to boolean
            $('#requestWait_' + reqId).html('Approved!');
            updateShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
}

function SendDataToModalInput(businessLocationId, shiftId) {
    var href = $('#btnYesModal').attr("href");
    var url = href.replace("param-id", shiftId).replace("param-buss", businessLocationId);

    $('#btnYesModal').attr("href", url);
}

function Dashboard_AcceptEmployerRequest(id) {
    {
        $.ajax({
            url: "/Manager/ApproveRequest",
            type: 'POST',
            headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            contentType: 'application/json',
            data: JSON.stringify({ reqId: id }),
            beforeSend: function (xhr) {
                //display spinner
                var reqId = JSON.parse(this.data)['reqId'];
                $('#requestBtns_' + reqId).hide();
                $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
            },
            success: function (msg) {
                //On success remove spinner
                var reqId = JSON.parse(this.data)['reqId'];
                $('#requestWait_' + reqId).html('Request Approved!');
                updateEmployerRequestCount();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert("Error: " + jqXHR.responseText);
                var reqId = JSON.parse(this.data)['reqId'];
                $('#requestWait_' + reqId).html('');
                $('#requestBtns_' + reqId).show();
            },
            statusCode: {
                404: function () {
                    alert("Error sending email: 404 page not found");
                }
            }
        })
    }
}
function Dashboard_RejectEmployerRequest(id) {
    $.ajax({
        url: "/Manager/RejectRequest",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ reqId: id }),
        beforeSend: function (xhr) {
            //display spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestBtns_' + reqId).hide();
            $('#requestWait_' + reqId).html('<img src="/images/ajax-loader.gif" alt="Wait" />');
        },
        success: function (msg) {
            //On success remove spinner
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('Rejected!');
            updateEmployerRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#requestWait_' + reqId).html('');
            $('#requestBtns_' + reqId).show();
        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
}
function sortNotificationsFunction(a, b) {
    var dateA = new Date(parseInt(a.CreatedDate.substr(6))).getTime();
    var dateB = new Date(parseInt(b.CreatedDate.substr(6))).getTime();
    return dateA > dateB ? 1 : -1;
};

function WeekView_RenderSpeeddialMenu() {

    var links = [
    {
        "bgcolor": "#1EC1A0",
        "icon": "+"
    },
    {
        "url": $('#btnMon').val(),
        "bgcolor": "#DB4A39",
        "color": "#fffff",
        "label": "Mo"
    },
    {
        "url": $('#btnTue').val(),
        "bgcolor": "#DB4A39",
        "color": "#fffff",
        "label": "Tu"
    },
    {
        "url": $('#btnWed').val(),
        "bgcolor": "#DB4A39",
        "color": "#fffff",
        "label": "We"
    },
    {
        "url": $('#btnThu').val(),
        "bgcolor": "#DB4A39",
        "color": "#fffff",
        "label": "Th"
    },
    {
        "url": $('#btnFri').val(),
        "bgcolor": "#DB4A39",
        "color": "#fffff",
        "label": "Fr"
    },
    {
        "url": $('#btnSat').val(),
        "bgcolor": "#DB4A39",
        "color": "#fffff",
        "label": "Sa"
    },
    {
        "url": $('#btnSun').val(),
        "bgcolor": "#DB4A39",
        "color": "#fffff",
        "label": "Su"
    }]

    $('.kc_fab_wrapper').kc_fab(links);


}

function UpdateNavMenu_NewBusiness() {
    if ($('#needRefresh').val() == 'True') {
        $.ajax({
            url: '/Partial/NavMenu',
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#leftMenu').replaceWith(content);
                $.getScript("../Scripts/bootstrap-theme.js");
            },
            error: function (ajaxObj, message, exceptionObj) {

            }
        });
    }
    $('#needRefresh').remove();
}

function ShiftTimes_OnChange() {

    $('#timeSection').datepair('refresh');

    var milliseconds = $('#timeSection').datepair('getTimeDiff');
    var div = $('#divShiftLength').text(timeConversion(milliseconds));
};

function timeConversion(millisec) {

    var seconds = (millisec / 1000).toFixed(1);

    var minutes = (millisec / (1000 * 60)).toFixed(1);

    var hours = (millisec / (1000 * 60 * 60)).toFixed(1);

    var days = (millisec / (1000 * 60 * 60 * 24)).toFixed(1);

    if (minutes == "NaN") {
        return ""
    } else if (seconds < 60) {
        return seconds + " Sec";
    } else if (minutes < 60) {
        return minutes + " Min";
    } else if (hours < 24) {
        return hours + " Hrs";
    } else {
        return days + " Days"
    }
}

//DEBUG FUNCTION ONLY, code not compiled into Release mode, only debug compile
function EmployeeIndex_Punchclock(employeeid, businessLocationId) {
    var dtInput = $("input[id='" +employeeid+" dtTimePunchTest']");
    $.ajax({
        url: "/Employee/PunchClock",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ employeeid: employeeid, businessLocationId: businessLocationId, timePunch: dtInput.val() }),
        success: function (msg) {
            alert(msg.message);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);

        },
        statusCode: {   
            404: function () {
                alert("Error sending Punch CLock: 404 page not found");
            }
        }
    })
}

function TimseheetEntryEditSubmit(formAction, formMethod, dataIn, modelId) {
    var btnId = $("button[type=submit][clicked=true]").attr('id');
    $.ajax({
        url: formAction,
        type: formMethod,
        data: dataIn,
        success: function (result) {
            if (result.success == "true") {

                $("#timeCard-" + modelId).html("<i class='glyphicon glyphicon-ok-circle'></i>");
                if (btnId == 'approve-btn-next') {
                    if (result.nextId == '') {
                        $('#modal-container').modal('hide');
                    }
                    else {
                        $('#modal-container .modal-body').html('<img src="/images/ajax-loader.gif" alt="Wait" style="display: block; margin: 100px auto;" />');
                        $('#empName').text('');
                        $.ajax({
                            url: "/Timesheet/EditTimesheetEntry/" + result.nextId,
                            type: 'GET',
                            cache: false,
                            success: function (data) {
                                $("#modal-container .modal-content").html(data);
                            },
                            error: function (e) {
                                alert('Error occurred: ' + e);
                            }
                        });
                    }
                }
                else {
                    $('#modal-container').modal('hide');
                }

            }
            else {
                // provide some highlighting or what have you or just set the message
                $('#result').addClass("validation-summary-errors").html(result.message);
            }
        }
    });
}


function TimseheetApproveSubmit(formAction, formMethod, dataIn, modelId) {
    $.ajax({
        url: formAction,
        type: formMethod,
        data: dataIn,
        success: function (result) {
            if (result.success == "true") {
                $('#modal-container').modal('hide');
                $('#approveTimesheetDiv').hide();
            }
            else {
                // provide some highlighting or what have you or just set the message
                $('#result').addClass("validation-summary-errors").html(result.message);
            }
        }
    });

}

function ShiftBlockTime_OnChange() {

    $('#timeSection').datepair('refresh');
    var start = $('#StartTime').val();
    var finish = $('#FinishTime').val();
    var milliseconds = $('#timeSection').datepair('getTimeDiff');

    if (finish < start && finish != '' && milliseconds > 0) {
        $('#FinishNextDay').prop('checked', true);
    }//Next day finish
    else {
        $('#FinishNextDay').prop('checked', false);
        //Same day finish
    }
};


function Manager_AddExternalUserAsEmployee() {
    
    var businessid = $("#BusinessID").val();
    $.ajax({
        url: "/Roster/AddAsEmployee",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ userId: $('#userId').val(), businessid: businessid }),
        beforeSend: function (xhr) {
            //display spinner
            var userId = JSON.parse(this.data)['userId'];
            $('#AddEmployee_' + userId).append(' <img src="/images/ajax-loader.gif" alt="Wait" />')
            // $('#requestWait_' + userId).html();
        },
        success: function (msg) {
            //On success remove spinner
            var userId = JSON.parse(this.data)['userId'];

            $("#modalAddEmployee").modal("hide");
            $('#AddEmployee_' + userId).prop('disabled', true);
            $('#AddEmployee_' + userId).html('Request Sent!');
            //if it is an open shift hide all other buttons once one is approved
            //updateExternalShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#AddEmployee_' + reqId).html('Add As Employee');

        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
}

function User_Recommendations() {
    $.ajax({
        url: "/Roster/UserRecommendations",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ RecommendedUserID: $('#RecommendedUserID').val() }),
        beforeSend: function (xhr) {
            //display spinner
            var RecommendedUserID = JSON.parse(this.data)['RecommendedUserID'];
            $('#Recommended_' + RecommendedUserID).append(' <img src="/images/ajax-loader.gif" alt="Wait" />')
            // $('#requestWait_' + userId).html();
        },
        success: function (msg) {
            //On success remove spinner
            var RecommendedUserID = JSON.parse(this.data)['RecommendedUserID'];

            $('#Recommended_' + RecommendedUserID).addClass('btn btn-primary btn-sm');
            $('#Recommended_' + RecommendedUserID).prop('disabled', true);
            $('#Recommended_' + RecommendedUserID).html('Recommended !');
            //if it is an open shift hide all other buttons once one is approved
            //updateExternalShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var RecommendedUserID = JSON.parse(this.data)['RecommendedUserID'];
            $('#Recommended_' + RecommendedUserID).html('Recommended');

        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
}

///---Contact a User-----
function ContactExternalUser() {
    $.ajax({
        url: "/Roster/ContactExternalUser",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ userId: $('#userId').val(), txtAcceptReason: $("#txtAcceptReason").val() }),
        beforeSend: function (xhr) {
            //display spinner
            var userId = JSON.parse(this.data)['userId'];
            $('#btnSend_' + userId).append(' <img src="/images/ajax-loader.gif" alt="Wait" />')
            // $('#requestWait_' + userId).html();
        },
        success: function (msg) {
            //On success remove spinner
            var userId = JSON.parse(this.data)['userId'];

            $("#modalMessage").modal("hide");
            //$('#btnContact_' + userId).prop('disabled', true);
            //$('#btnContact_' + userId).html('Request Sent!');
            //if it is an open shift hide all other buttons once one is approved
            //updateExternalShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var reqId = JSON.parse(this.data)['reqId'];
            $('#btnContact_' + reqId).html('Add As Employee');

        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
}

function User_EndorseASkill(id) {
    var userId = $('#RecommendedUserID').val()
    $.ajax({
        url: "/Roster/UserEndorseASkill",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ userId: $('#RecommendedUserID').val(), SkillID: id }),
        beforeSend: function (xhr) {
            //display spinner
            var userId = JSON.parse(this.data)['userId'];
            $('#' + id).append(' <img src="/images/ajax-loader.gif" alt="Wait" />')
            // $('#requestWait_' + userId).html();
        },
        success: function (msg) {
            //On success remove spinner
            var userId = JSON.parse(this.data)['userId'];

            $('#' + id).addClass('btn btn-primary btn-sm');
            $('#' + id).prop('disabled', true);
            $('#' + id).html('Endorsed !');
            //if it is an open shift hide all other buttons once one is approved
            //updateExternalShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var userId = JSON.parse(this.data)['userId'];
            $('#' + id).html('Endorse a skill');

        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
}

function getLocaleDateString() {

    var formats = {
        "en-US": "mm/dd/yyyy",
        "en-AU": "dd/mm/yyyy",
        "US": "mm/dd/yyyy",
        "AU": "dd/mm/yyyy"
    };
  
    return formats[countryConfig] || 'dd/mm/yyyy';

}

function WeekViewBudget_Calculate(id) {
    var userId = $('#RecommendedUserID').val()
    $.ajax({
        url: "/Roster/UserEndorseASkill",
        type: 'POST',
        headers: { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        contentType: 'application/json',
        data: JSON.stringify({ userId: $('#RecommendedUserID').val(), SkillID: id }),
        beforeSend: function (xhr) {
            //display spinner
            var userId = JSON.parse(this.data)['userId'];
            $('#' + id).append(' <img src="/images/ajax-loader.gif" alt="Wait" />')
            // $('#requestWait_' + userId).html();
        },
        success: function (msg) {
            //On success remove spinner
            var userId = JSON.parse(this.data)['userId'];

            $('#' + id).addClass('btn btn-primary btn-sm');
            $('#' + id).prop('disabled', true);
            $('#' + id).html('Endorsed !');
            //if it is an open shift hide all other buttons once one is approved
            //updateExternalShiftRequestCount();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error: " + jqXHR.responseText);
            var userId = JSON.parse(this.data)['userId'];
            $('#' + id).html('Endorse a skill');

        },
        statusCode: {
            404: function () {
                alert("Error sending email: 404 page not found");
            }
        }
    })
}
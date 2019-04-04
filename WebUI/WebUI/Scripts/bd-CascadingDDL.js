
jQuery.validator.unobtrusive.adapters.add("dropdown", function (options) {
    //  
    if (options.element.tagName.toUpperCase() == "SELECT" && options.element.type.toUpperCase() == "SELECT-ONE") {
        options.rules["required"] = true;
       if (options.message) {
            options.messages["required"] = options.message;
        }
    }
});



$(function () {

    
    //$('#RoleId').change(function () {
    //    //  
    //    var roleId = $("#RoleId :selected").val();
    //    var businessId = $("#BusinessId").val();
    //    if (roleId != "" && businessId != "") {
    //        $.ajax({
    //            type: "GET",
    //            contentType: "application/json; charset=utf-8",
    //            url: '/Roster/GetEmployeesForRole',
    //            data: { "roleId": roleId, "businessId": businessId },
    //            dataType: "json",
    //            beforeSend: function () {
    //                //alert('here1:' + roleId);
    //            },
    //            success: function (data) {
    //                var items = "<option value=''>-- SELECT --</option>";
    //                $.each(data, function (i, employee) {
    //                    items += "<option value='" + employee.Id + "'>" + employee.FullName + "</option>";
    //                });
    //                $('#EmployeeId').html(items);
    //            },
    //            error: function (result) {
    //                alert('Service call failed: ' + result.status + ' Type :' + result.statusText);
    //            }
    //        });
    //    }
    //    else {
    //        var items = "<option value=''>-- SELECT --</option>";
    //        $('#EmployeeId').html(items);
    //    }
    //});

});
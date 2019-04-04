
    $("#dialogDiv").dialog({
              modal: true,
              autoOpen: false,
              width: 600,
              buttons: {
                  "Ok": function () { $(this).dialog("close"); },
                  "Cancel": function () { $(this).dialog("close"); }
              }
          });

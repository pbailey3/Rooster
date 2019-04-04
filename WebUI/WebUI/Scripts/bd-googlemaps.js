
var placesearch, autocomplete;
var componentForm = {
    subpremise: 'short_name',
    street_number: 'short_name',
    route: 'long_name',
    locality: 'long_name',
    administrative_area_level_1: 'short_name',
    country: 'long_name',
    postal_code: 'short_name'
};

function initialize() {
    // Create the autocomplete object, restricting the search
    // to geographical location types.
    var countryConfig = document.getElementById('hdnCountryConfig');
    autocomplete = new google.maps.places.Autocomplete(
    (document.getElementById('addrAuto')),
    { types: ['geocode'], componentRestrictions: { country: countryConfig.value } });
    // When the user selects an address from the dropdown,
    // populate the address fields in the form.
    google.maps.event.addListener(autocomplete, 'place_changed', function () {
        fillInAddress();
    });
}

// [START region_fillform]
function fillInAddress() {
    
    // Get the place details from the autocomplete object.
    var place = autocomplete.getPlace();

    for (var component in componentForm) {
        document.getElementById(component).value = '';
        document.getElementById(component).disabled = false;
    }

    var curLon = place.geometry.location.lng();
    var curLat = place.geometry.location.lat();

    // Get each component of the address from the place details
    // and fill the corresponding field on the form.
    for (var i = 0; i < place.address_components.length; i++) {
        var addressType = place.address_components[i].types[0];
        if (componentForm[addressType]) {
            var val = place.address_components[i][componentForm[addressType]];
            document.getElementById(addressType).value = val;
        }
    }

    $("input[id$='Address_Line1']").val(document.getElementById('subpremise').value
                                                        + " " + document.getElementById('street_number').value
                                                        + " " + document.getElementById('route').value);

    $("input[id$='Address_Suburb']").val(document.getElementById('locality').value);
    var state = document.getElementById('administrative_area_level_1').value;

    console.log(document.getElementById('subpremise').value + " & " + document.getElementById('street_number').value + " & " + document.getElementById('route').value + " & " + document.getElementById('locality').value + " & " + document.getElementById('postal_code').value);

    $("select[id$='Address_State'] option[value='" + state + "']").prop('selected', true);

    $("input[id$='Address_Postcode']").val(document.getElementById('postal_code').value);

    $("input[id$='Address_Long']").val(curLon);
    $("input[id$='Address_Lat']").val(curLat);
    $("input[id$='Address_PlaceId']").val(place.id);

    $("input[id$='Address_PlaceLongitude']").val(curLon);
    $("input[id$='Address_PlaceLatitude']").val(curLat);
    $("input[id$='Address_State']").val(state);
}

// [END region_fillform]

// [START region_geolocation]
// Bias the autocomplete object to the user's geographical location,
// as supplied by the browser's 'navigator.geolocation' object.
function geolocate() {
    if (navigator.geolocation && autocomplete) {
        navigator.geolocation.getCurrentPosition(function (position) {
            var geolocation = new google.maps.LatLng(
            position.coords.latitude, position.coords.longitude);
            autocomplete.setBounds(new google.maps.LatLngBounds(geolocation,
            geolocation));
        });
    }
}
// [END region_geolocation]

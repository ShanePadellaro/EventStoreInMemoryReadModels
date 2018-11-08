fromCategory('Account')
    .when({
        $init: function(){
            return {
                count: 0,
                taxes:{}
            }
        },
        "Credit": function(state, event) {
            state.count += 1;
            calculateTaxes(state.taxes,event.data)
        },
        "Debit": function(state, event) {
            state.count += 1;

        }
    })

function calculateTaxes(taxes,transaction)
{
    var billingDate = new Date(transaction.BillingDate);
    var year = billingDate.getFullYear();
    var month = billingDate.getMonth();
    var day = billingDate.getDate();
    var tax = transaction.Tax;
    if(!typeof transaction.CountryCode === "string")
        throw "CountryCode is missing"

    var countryCode = transaction.CountryCode.toUpperCase();

    if(!taxes[countryCode]){
        taxes[countryCode] = {};
    }
    if(!taxes[countryCode][year]){
        taxes[countryCode][year] = {};
    }

    if(!taxes[countryCode][year][month]){
        taxes[countryCode][year][month] = tax;
    }
    else{
        taxes[countryCode][year][month] += tax;
    }


}
fromCategory('Account')
    .foreachStream()
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
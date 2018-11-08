fromCategory('Account')
    .foreachStream()
    .when({
        $init: function(){
            return {
                count: 0,
                balance: 0
            }
        },
        "Credit": function(state, event) {
            state.count += 1;
            state.balance += event.data.Amount;
        },
        "Debit": function(state, event) {
            state.count += 1;
            state.balance -= event.data.Amount;
        }
    })

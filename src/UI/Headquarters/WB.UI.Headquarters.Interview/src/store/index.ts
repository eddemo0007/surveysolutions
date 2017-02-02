import * as Vue from "vue"
import * as Vuex from "vuex"

import * as moment from "moment"
import { safeStore } from "../errors"
import actions from "./store.actions"
import fetch from "./store.fetch"
import mutations from "./store.mutations"
import sidebar from "./store.sidebar"

const store = new Vuex.Store(safeStore({
    modules: { fetch, sidebar },
    state: {
        lastActivityTimestamp: moment(),
        entities: [], /* IInterviewEntity[] */
        entityDetails: { /* string: object */ },
        breadcrumbs: {
            breadcrumbs: []
        },
        sidebar: [ /* ISidebarPanel */]
    },
    actions,
    mutations
}))

export default store

import Vue from 'vue'
import VueRouter from "vue-router"
import VueResource from "vue-resource"
import startup from "startup"
import App from "interviewer/InterviewsApp"
import InterviewsTable from "components/InterviewsTable"

Vue.component("InterviewsTable", InterviewsTable)
Vue.use(VueRouter)
Vue.use(VueResource)
Vue.http.headers.common['Authorization'] = input.settings.acsrf.token;

const router = new VueRouter({
    base: Vue.prototype.$config.interviewerHqEndpoint + "/",
    mode: "history",
    routes: [
        {
        path: "/Started",
        component: InterviewsTable,
        props: {
            statuses: ['InterviewerAssigned']
        },
         caseSensitive: false 
    },   {
        path: "/Rejected",
        component: InterviewsTable,
        props: {
            statuses: ['RejectedBySupervisor', 'RejectedByHeadquarters'] 
        },
         caseSensitive: false 
    },   {
        path: "/Completed",
        component: InterviewsTable,
        props: {
            statuses: ['Completed']
        },
         caseSensitive: false 
    }
    ]
})

startup(App, {
    router
})
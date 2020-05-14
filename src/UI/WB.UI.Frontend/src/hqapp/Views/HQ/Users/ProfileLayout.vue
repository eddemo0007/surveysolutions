<template>
    <HqLayout :hasFilter="false">
        <div slot="filters">
            <div v-if="successMessage != null"
                id="alerts"
                class="alerts">
                <div class="alert alert-success">
                    <button class="close"
                        data-dismiss="alert"
                        aria-hidden="true">
                        ×
                    </button>
                    {{successMessage}}
                </div>
            </div>
        </div>
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a v-bind:href="referrerUrl">{{referrerTitle}}</a>
                </li>
            </ol>
            <h1>{{$t('Strings.HQ_Views_Manage_Title')}}<b v-if="!isOwnProfile">
                : {{userName}}
            </b></h1>
        </div>
        <div class="extra-margin-bottom">
            <div class="profile">
                <ul class="nav nav-tabs extra-margin-bottom">
                    <li class="nav-item"
                        v-bind:class=" {'active': currentTab == 'account'}" >
                        <a class="nav-link"
                            id="profile"
                            v-bind:href="getUrl('../../Users/Manage')">{{$t('Pages.AccountManage_Profile')}}</a>
                    </li>
                    <li class="nav-item"
                        v-bind:class="{'active': currentTab=='password'}">
                        <a class="nav-link"
                            id="password"
                            v-bind:href="getUrl('../../Users/ChangePassword')">{{$t('Pages.AccountManage_ChangePassword')}}</a>
                    </li>
                    <li class="nav-item"
                        v-bind:class="{'active': currentTab=='two-factor'}">
                        <a class="nav-link"
                            id="two-factor"
                            v-bind:href="getUrl('../../Users/TwoFactorAuthentication')">{{$t('Pages.AccountManage_TwoFactorAuth')}}</a>
                    </li>
                </ul>

                <div class="col-sm-12">
                    <slot />
                </div>
            </div>
        </div>
    </HqLayout>
</template>
<script>
export default {
    props:{
        role: String,
        successMessage: String,
        isOwnProfile: Boolean,
        userName: String,
        userId: String,
        currentTab: {
            type: String,
            required: true,
        },
    },
    computed:{
        isAdmin() {
            return this.role == 'Administrator'
        },
        isHeadquarters() {
            return this.role == 'Headquarter'
        },
        isSupervisor() {
            return this.role == 'Supervisor'
        },
        isInterviewer() {
            return this.role == 'Interviewer'
        },
        isObserver() {
            return this.role == 'Observer'
        },
        isApiUser() {
            return this.role == 'ApiUser'
        },
        canLockBySupervisor() {
            return this.isInterviewer
        },
        referrerTitle() {
            if (!this.isOwnProfile) {
                if (this.isHeadquarters) return this.$t('Pages.Profile_HeadquartersList')
                if (this.isSupervisor) return this.$t('Pages.Profile_SupervisorsList')
                if (this.isInterviewer) return this.$t('Pages.Profile_InterviewerProfile')
                if (this.isObserver) return this.$t('Pages.Profile_ObserversList')
                if (this.isApiUser) return this.$t('Pages.Profile_ApiUsersList')
            }

            return this.$t('Pages.Home')
        },
        referrerUrl() {
            if (!this.isOwnProfile) {
                if (this.isHeadquarters) return '../../Headquarters'
                if (this.isSupervisor) return '../../Supervisors'
                if (this.isInterviewer) return '../../Interviewer/Profile/' + this.userId
                if (this.isObserver) return '../../Observers'
                if (this.isApiUser) return '../../ApiUsers'
            }

            return '/'
        },

    },
    methods:{
        getUrl: function(baseUrl){
            if(this.isOwnProfile)
                return baseUrl
            else
                return baseUrl + '/' + this.userId

        },
    },
}
</script>
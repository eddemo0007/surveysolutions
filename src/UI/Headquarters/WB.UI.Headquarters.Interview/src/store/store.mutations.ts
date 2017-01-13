import * as Vue from "vue"
import * as Vuex from "vuex"

export default {
    SET_QUESTIONNAIRE_INFO(state, questionnaireInfo: IQuestionnaireInfo) {
        state.questionnaire = questionnaireInfo;
    },
    SET_PREFILLED_QUESTIONS(state, prefilledPageData: IPrefilledPageData) {
        state.prefilledQuestions = prefilledPageData.questions
        state.firstSectionId = prefilledPageData.firstSectionId
    },
    SET_ENTITY_DETAILS(state, entity) {
        Vue.set(state.entityDetails, entity.id, entity)
    },
    SET_SECTION(state, section) {
        state.section = section
    },
    SET_ANSWER_NOT_SAVED(state, {id, message}) {
        const validity = state.entityDetails[id].validity
        Vue.set(validity, "errorMessage", true)
        validity.messages = [message]
        validity.isValid = false
    }
}

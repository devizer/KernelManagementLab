import dispatcher from "./NavDispatcher";

export const NAV_UPDATED_ACTION = "NAV_UPDATED_ACTION";

export function NavUpdated(navTitle) {
    dispatcher.dispatch({
        type: NAV_UPDATED_ACTION,
        value: navTitle
    })
}

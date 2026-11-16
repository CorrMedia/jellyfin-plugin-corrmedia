import { ServerConnections } from 'lib/jellyfin-apiclient';
import Events from 'utils/events';

const sidecarIds = new Set();
const sidecarListeners = new Set();
const editedSuffix = ' (Edited)';

export function subscribeCorrSidecars(listener) {
    sidecarListeners.add(listener);
    return () => {
        sidecarListeners.delete(listener);
    };
}

export function itemHasCorrSidecar(item) {
    if (!item) {
        return false;
    }

    if (item.HasCorrSidecar) {
        return true;
    }

    const id = item.Id || item.ItemId;
    if (id && sidecarIds.has(String(id).toLowerCase())) {
        return true;
    }

    const name = item.Name || '';
    return name.endsWith(editedSuffix);
}

function rememberIds(ids) {
    sidecarIds.clear();
    (ids || []).forEach(id => {
        if (id) {
            sidecarIds.add(String(id).toLowerCase());
        }
    });
    sidecarListeners.forEach(listener => listener());
}

function stampVisibleCards() {
    const cards = document.querySelectorAll('.card[data-id], .listItem[data-id]');
    cards.forEach(card => {
        const id = (card.getAttribute('data-id') || '').toLowerCase();
        if (!id || !sidecarIds.has(id)) {
            return;
        }

        card.classList.add(card.classList.contains('listItem') ? 'listItem-hasCorrSidecar' : 'card-hasCorrSidecar');

        const indicators = card.querySelector('.cardIndicators, .listItemIndicators');
        if (!indicators || indicators.querySelector('.corrSidecarIndicator')) {
            return;
        }

        const badge = document.createElement('div');
        badge.className = 'corrSidecarIndicator';
        badge.title = 'Edit file available';
        badge.textContent = 'Edits';
        indicators.prepend(badge);
    });
}

function refreshSidecars() {
    const apiClient = ServerConnections.currentApiClient();
    if (!apiClient || !apiClient.accessToken()) {
        return;
    }

    apiClient.getJSON(apiClient.getUrl('CorrMedia/Sidecars')).then(result => {
        rememberIds(result?.Ids || result?.ids);
        stampVisibleCards();
    }).catch(() => {
        // The installed server hides the plugin until it is restarted onto a compatible build.
    });
}

Events.on(ServerConnections, 'localusersignedin', refreshSidecars);
refreshSidecars();
window.setTimeout(refreshSidecars, 2500);
window.setTimeout(refreshSidecars, 8000);
window.setTimeout(refreshSidecars, 25000);

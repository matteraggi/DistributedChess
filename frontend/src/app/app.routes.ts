import { Routes } from '@angular/router';
import { LobbyPage } from './pages/lobby/lobby';

export const routes: Routes = [
    { path: '', redirectTo: 'lobby', pathMatch: 'full' }, // redirect automatico alla lobby
    { path: 'lobby', component: LobbyPage },
    { path: 'game/:id', loadComponent: () => import('./pages/game/game').then(m => m.Game) }
];

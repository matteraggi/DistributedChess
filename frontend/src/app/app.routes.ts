import { Routes } from '@angular/router';
import { LobbyPage } from './pages/lobby/lobby';
import { Board } from './pages/board/board';

export const routes: Routes = [
    { path: '', redirectTo: 'lobby', pathMatch: 'full' }, // redirect automatico alla lobby
    { path: 'lobby', component: LobbyPage },
    { path: 'game/:id', loadComponent: () => import('./pages/game/game').then(m => m.Game) },
    { path: 'board/:id', component: Board },
    { path: '**', redirectTo: 'lobby' }
];

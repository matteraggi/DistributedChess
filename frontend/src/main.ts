import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { WebsocketService } from './app/services/websocket.service';

bootstrapApplication(App, appConfig)
  .then(appRef => {
    const ws = appRef.injector.get(WebsocketService);
    ws.connect();
  })
  .catch(err => console.error(err));

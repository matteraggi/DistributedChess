import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LobbyPage } from './lobby';


describe('Lobby', () => {
  let component: LobbyPage;
  let fixture: ComponentFixture<LobbyPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LobbyPage]
    })
      .compileComponents();

    fixture = TestBed.createComponent(LobbyPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChessQueen } from './chess-queen';

describe('ChessQueen', () => {
  let component: ChessQueen;
  let fixture: ComponentFixture<ChessQueen>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChessQueen]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ChessQueen);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

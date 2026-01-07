import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MinhasAvaliacoes } from './minhas-avaliacoes';

describe('MinhasAvaliacoes', () => {
  let component: MinhasAvaliacoes;
  let fixture: ComponentFixture<MinhasAvaliacoes>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MinhasAvaliacoes]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MinhasAvaliacoes);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
